using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Syborg.CommandResults;
using Syborg.Common;
using Syborg.Razor;
using Syborg.Routing;

namespace Syborg.Commands
{
    public class RobotsTxtCommand : IWebCommand
    {
        public RobotsTxtCommand(IEnumerable<string> disallows, string sitemapRelativeUrl)
        {
            Contract.Requires(disallows != null);
            this.disallows.AddRange(disallows);
            this.sitemapRelativeUrl = sitemapRelativeUrl;
        }

        public IWebCommandResult Execute(IWebContext context, WebRequestRouteMatch routeMatch)
        {
            UrlHelper url = new UrlHelper(context.ApplicationUrl);
            StringBuilder s = new StringBuilder ();

            s.AppendLine (@"User-agent: *");

            foreach (string disallow in disallows)
                s.AppendLine (@"Disallow: {0}".Fmt(url.Base() + disallow));

            if (sitemapRelativeUrl != null)
                s.AppendLine (@"Sitemap: {0}".Fmt (url.AbsoluteBase() + sitemapRelativeUrl));

            TextResult result = new TextResult(s.ToString());
            result.AddHeader (HttpConsts.HeaderContentType, HttpConsts.ContentTypeTextPlainUtf8);
            result.LoggingSeverity = LoggingSeverity.Verbose;
            return result;
        }

        private readonly List<string> disallows = new List<string>();
        private readonly string sitemapRelativeUrl;
    }
}