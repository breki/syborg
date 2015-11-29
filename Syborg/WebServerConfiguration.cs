using System;

namespace Syborg
{
    public class WebServerConfiguration : IWebServerConfiguration
    {
        public HttpsMode HttpsMode
        {
            get { return httpsMode; }
            set { httpsMode = value; }
        }

        public int? HttpsPort { get; set; }

        public TimeSpan SimulatedResponseLag
        {
            get { return simulatedResponseLag; }
            set { simulatedResponseLag = value; }
        }

        public bool UseHsts { get; set; }
        public bool WebServerDevelopmentMode { get; set; }
        public string WebServerDevelopmentRootDir { get; set; }

        private HttpsMode httpsMode = HttpsMode.AllowBoth;
        private TimeSpan simulatedResponseLag = TimeSpan.Zero;
    }
}