using System;

namespace Syborg.Common
{
    public class TimeMachine : ITimeService
    {
        public DateTime CurrentTime
        {
            get { return DateTime.Now + timeOffset; }
        }

        public DateTime CurrentTimeUtc
        {
            get { return DateTime.UtcNow + timeOffset; }
        }

        public void OffsetTime (TimeSpan newOffset)
        {
            timeOffset = newOffset;
        }

        public void Wait (TimeSpan timeSpan)
        {
            timeOffset += timeSpan;
        }

        private TimeSpan timeOffset = TimeSpan.Zero;
    }
}