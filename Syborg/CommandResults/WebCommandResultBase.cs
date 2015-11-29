using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Syborg.Common;

namespace Syborg.CommandResults
{
    public abstract class WebCommandResultBase : IWebCommandResult
    {
        public int? StatusCode { get; set; }

        public NameValueCollection Headers
        {
            get { return headers; }
        }

        public LoggingSeverity? LoggingSeverity
        {
            get { return loggingSeverity; }
            set { loggingSeverity = value; }
        }

        public void AddHeader (string name, string value)
        {
            headers.Add(name, value);
        }

        public abstract void Apply (IWebContext context);

        protected void ApplyEssentials(IWebContext context)
        {
            if (StatusCode.HasValue)
            {
                context.StatusCode = StatusCode.Value;
                if (StatusCode.Value >= 400)
                    log.DebugFormat("Status code: {0}", StatusCode);
            }

            foreach (string header in Headers)
                context.AddHeader (header, Headers[header]);

            if (LoggingSeverity.HasValue)
                context.LoggingSeverity = LoggingSeverity.Value;
        }

        [Obsolete]
        protected void VerifyCaching(IWebContext context)
        {
            Contract.Requires(context != null);
            if (context.Configuration.WebServerDevelopmentMode)
            {
                //string[] cacheControlValues = context.ResponseHeaders.GetValues(HttpConsts.HeaderCacheControl);
                //if (cacheControlValues == null || !cacheControlValues.AsQueryable ().Any (x => x == HttpConsts.CacheControlNoStore))
                //    throw new InvalidOperationException ("The response does not have {0}={1} header".Fmt (HttpConsts.HeaderCacheControl, HttpConsts.CacheControlNoStore));
            }
            else
            {
                // verify caching headers
                //string[] cacheControlValues = context.ResponseHeaders.GetValues (HttpConsts.HeaderCacheControl);
                //if (cacheControlValues == null || !cacheControlValues.AsQueryable ().Any (
                //    x => x == HttpConsts.CacheControlNoStore || x == HttpConsts.CacheControlNoCache || x == HttpConsts.CacheControlPrivate || x == HttpConsts.CacheControlPublic))
                //    throw new InvalidOperationException ("The response does not have on of {0} headers".Fmt (HttpConsts.HeaderCacheControl));

                string etag = context.ResponseHeaders[HttpConsts.HeaderETag];
                if (!string.IsNullOrEmpty(etag))
                {
                    if (!etagRegex.IsMatch(etag))
                        throw new InvalidOperationException("ETag '{0}' syntax isn't valid".Fmt(etag));
                }
            }
        }

        private readonly NameValueCollection headers = new NameValueCollection();
        private readonly Regex etagRegex = new Regex(@"^\""[^\\]*\""$", RegexOptions.Compiled);
        private LoggingSeverity? loggingSeverity;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}