using AotServiceProvider;
using Authi.Server.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Authi.Server.Services
{
    public record AppHealthEvent(DateTimeOffset DateTimeOffset, string ExceptionType, string Message, string? StackTrace = null);

    [Service]
    internal interface IAppHealthMonitor
    {
        void ReportEvent(Exception exception);
        void Flush();
        IReadOnlyCollection<AppHealthEvent> GetEvents();
    }

    internal class AppHealthMonitor : IAppHealthMonitor
    {
        internal static string LogFilePath => Path.Combine(LogFolderPath, "eventLog.jsonl");
        internal static string LogFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        internal const int MaxEvents = 100;

        private readonly ConcurrentQueue<AppHealthEvent> _events = [];

        public AppHealthMonitor()
        {
            try
            {
                if (!Directory.Exists(LogFolderPath))
                {
                    Directory.CreateDirectory(LogFolderPath);
                }
                if (File.Exists(LogFilePath))
                {
                    var lines = File.ReadLines(LogFilePath).TakeLast(MaxEvents);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var e = JsonSerializer.Deserialize<AppHealthEvent>(line);
                            if (e != null) _events.Enqueue(e);
                        }
                        catch (JsonException)
                        {
                            Debug.WriteLine($"{nameof(AppHealthMonitor)}: Failed to parse an event from line [{line}]");
                        }
                    }
                }
            }
            catch
            {
                Debug.WriteLine($"{nameof(AppHealthMonitor)}: Failed to read the event log file [{LogFilePath}]");
            }
        }

        public void ReportEvent(Exception exception)
        {
            var time = DateTimeOffset.UtcNow;
            if (exception is InterceptedDbError dbError && dbError.InnerException is Exception innerException)
            {
                _events.Enqueue(new AppHealthEvent(time, dbError.GetType().Name, innerException.Message, dbError.CommandText));
            }
            else
            {
                _events.Enqueue(new AppHealthEvent(time, exception.GetType().Name, exception.Message, exception.StackTrace));
            }

            while (_events.Count > MaxEvents)
            {
                _events.TryDequeue(out _);
            }
        }

        public void Flush()
        {
            try
            {
                var lines = _events.Select(e => JsonSerializer.Serialize(e));
                File.WriteAllLines(LogFilePath, lines);
            }
            catch
            {
                Debug.WriteLine($"{nameof(AppHealthMonitor)}: Failed to write the event log file [{LogFilePath}]");
            }
        }

        public IReadOnlyCollection<AppHealthEvent> GetEvents()
        {
            return _events.OrderByDescending(e => e.DateTimeOffset).ToList();
        }
    }
}