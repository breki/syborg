using System;

namespace Syborg.Common
{
    public interface IWaitHandle : IDisposable
    {
        bool Wait ();
        bool Wait (TimeSpan timeout);
    }
}