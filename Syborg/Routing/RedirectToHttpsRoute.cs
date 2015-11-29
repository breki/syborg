using System;
using System.Net;
using System.Reflection;
using log4net;
using Syborg.CommandResults;

namespace Syborg.Routing
{
    public class RedirectToHttpsRoute : IWebRequestRoute
    {
        public bool ProcessIfMatch(IWebContext context, out IWebCommandResult result)
        {
            //log.DebugFormat("ApplicationUrl={0}, Url={1}, RawUrl={2}, ApplicationPath={3}", context.ApplicationUrl, context.Url, context.RawUrl, context.ApplicationPath);
            
            result = null;

            IWebServerConfiguration configuration = context.Configuration;

            if (context.IsSecureConnection)
                return false;

            if (configuration.HttpsMode == HttpsMode.AllowBoth)
                return false;

            if (configuration.HttpsMode == HttpsMode.RequireHttpsExceptLocal)
            {
                if (context.IsRequestLocal)
                    return false;
            }

            HttpStatusResult httpStatusResult = new HttpStatusResult(HttpStatusCode.MovedPermanently);

            UriBuilder uriBuilder = new UriBuilder(context.Url);
            uriBuilder.Scheme = "https";

            if (configuration.HttpsPort.HasValue)
                uriBuilder.Port = configuration.HttpsPort.Value;
            else
                uriBuilder.Port = -1;

            string destinationUrl = uriBuilder.ToString();
            httpStatusResult.AddHeader(HttpConsts.HeaderLocation, destinationUrl);

            if (log.IsDebugEnabled)
                log.DebugFormat("Redirecting request '{0}' to '{1}'", context.ApplicationUrl, destinationUrl);

            result = httpStatusResult;
            return true;
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}