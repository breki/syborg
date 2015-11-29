using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using log4net;
using Syborg.CommandResults;
using Syborg.Common;

namespace Syborg.Routing
{
    public class RegexWebRequestRoute : IWebRequestRoute
    {
        public RegexWebRequestRoute(string routeRegex, HttpMethod httpMethod, IWebCommand command)
        {
            Contract.Requires(routeRegex != null);
            Contract.Requires(command != null);
            this.routeRegex = new Regex(routeRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this.httpMethod = httpMethod.ToString();
            this.command = command;
        }

        public RegexWebRequestRoute(string routeRegex, string httpMethod, IWebCommand command)
        {
            Contract.Requires(routeRegex != null);
            Contract.Requires(command != null);
            this.routeRegex = new Regex(routeRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this.httpMethod = httpMethod;
            this.command = command;
        }

        public bool ProcessIfMatch (IWebContext context, out IWebCommandResult result)
        {
            result = null;

            if (context.HttpMethod != httpMethod)
                return false;

            UriBuilder urlBuilder = new UriBuilder (context.Url);

            // ensure the URL path is decoded
            string urlPath = HttpUtility.UrlDecode(urlBuilder.Path);

            string applicationPath = context.ApplicationPath;

            if (applicationPath != null)
            {
                if (!urlPath.StartsWith(applicationPath, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                bool endsWithSlash = applicationPath.EndsWith("/");
                int startIndex = applicationPath.Length - (endsWithSlash ? 1 : 0);
                if (startIndex > urlPath.Length)
                    return false;

                urlPath = urlPath.Substring(startIndex);
            }

            if (urlPath.Length > 1)
            {
                int endCharsCutOff = 0;
                if (urlPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    endCharsCutOff++;

                urlPath = urlPath.Substring(1, urlPath.Length - (1 + endCharsCutOff));
            }
            else
            {
                if (urlPath.Length >= 1 && urlPath[0] != '/')
                    throw new InvalidOperationException("URL path is invalid: '{0}'".Fmt(urlPath));
                urlPath = string.Empty;
            }

            Match match = routeRegex.Match (urlPath);
            if (!match.Success)
                return false;

            if (log.IsDebugEnabled)
                log.DebugFormat("Executing web command {0}", command);

            WebRequestRouteMatch routeMatch = WebRequestRouteMatch.FromRegexMatch(routeRegex, match);
            result = command.Execute(context, routeMatch);

            return true;
        }

        private readonly Regex routeRegex;
        private readonly string httpMethod;
        private readonly IWebCommand command;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}