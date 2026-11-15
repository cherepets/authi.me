using AotServiceProvider;
using System;

namespace Authi.Common.Services
{
    [Service]
    public interface IClock
    {
        DateTimeOffset UniversalTime { get; }
        public long Timestamp => UniversalTime.ToUnixTimeMilliseconds();

        public bool IsRecent(long timestamp, TimeSpan threshold)
            => IsRecent(timestamp, threshold.TotalMilliseconds);

        public bool IsRecent(long timestamp, double thresholdMilliseconds)
        {
            return Math.Abs(Timestamp - timestamp) < thresholdMilliseconds;
        }
    }

    public class Clock : IClock
    {
        public DateTimeOffset UniversalTime => DateTimeOffset.UtcNow;
    }
}
