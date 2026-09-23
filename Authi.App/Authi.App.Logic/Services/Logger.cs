using Authi.Common.Services;
using System;
using System.Diagnostics;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface ILogger
    {
        void Write(string message);
        void Write(Exception exception) => Write(exception.ToString());
    }

    internal class Logger : ILogger
    {
        public void Write(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
