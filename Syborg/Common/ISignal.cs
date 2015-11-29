namespace Syborg.Common
{
    public interface ISignal : IWaitHandle
    {
        void Set ();
        void Reset ();
    }
}