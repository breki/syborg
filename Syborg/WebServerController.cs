using Syborg.Common;

namespace Syborg
{
    public class WebServerController : IWebServerController
    {
        public WebServerController(ISignal serverStopSignal)
        {
            this.serverStopSignal = serverStopSignal;
        }

        public bool WasAborted
        {
            get { return wasAborted; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        public void SignalToStop()
        {
            serverStopSignal.Set();
        }

        public void SignalToAbort(string errorMessage)
        {
            this.errorMessage = errorMessage;
            wasAborted = true;
            serverStopSignal.Set ();
        }

        private readonly ISignal serverStopSignal;
        private string errorMessage;
        private bool wasAborted;
    }
}