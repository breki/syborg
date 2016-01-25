using System;
using System.Reflection;
using System.Web;
using log4net;
using Syborg.Hosting;

namespace Syborg.WebTests
{
    public class Global : HttpApplication
    {
        protected void Application_Start (object sender, EventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure ();

            if (log.IsDebugEnabled)
                log.Debug ("Application_Start");

            appHost = new SyborgTestHttpModuleAppHost();
            Application[SyborgHttpModule.KeyAppHost] = appHost;
        }

        protected void Session_Start (object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest (object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest (object sender, EventArgs e)
        {
        }

        protected void Application_Error (object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError ();
            log.ErrorFormat ("Application_Error: {0}", ex);
        }

        protected void Session_End (object sender, EventArgs e)
        {
        }

        protected void Application_End (object sender, EventArgs e)
        {
            if (log.IsDebugEnabled)
                log.Debug ("Application_End");

            if (appHost != null)
                appHost.Dispose ();
        }

        private SyborgTestHttpModuleAppHost appHost;
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
    }
}
