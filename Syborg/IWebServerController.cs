namespace Syborg
{
    public interface IWebServerController
    {
        bool WasAborted { get; }
        string ErrorMessage { get; }

        void SignalToStop ();
        void SignalToAbort(string errorMessage);
    }
}