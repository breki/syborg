namespace Syborg.Common
{
    public interface ISignal : IWaitHandle
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
        void Set ();
        void Reset ();
    }
}