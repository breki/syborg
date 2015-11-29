using System;

namespace Syborg.Common
{
    public interface ITimeService
    {
        DateTime CurrentTime { get; }
        DateTime CurrentTimeUtc { get; }

        void Wait (TimeSpan timeSpan);
    }
}