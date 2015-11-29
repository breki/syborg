namespace Syborg
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
    public interface IWebServerController
    {
        bool WasAborted { get; }
        string ErrorMessage { get; }

        void SignalToStop ();
        void SignalToAbort(string errorMessage);
    }
}