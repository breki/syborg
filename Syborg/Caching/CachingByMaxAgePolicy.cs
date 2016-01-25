using System;
using System.Net;
using LibroLib;

namespace Syborg.Caching
{
    public class CachingByMaxAgePolicy : ICachingPolicy
    {
        public CachingByMaxAgePolicy(TimeSpan maxAge, Func<object, DateTime?> lastModifiedFunc)
        {
            this.maxAge = maxAge;
            this.lastModifiedFunc = lastModifiedFunc;
        }

        public TimeSpan MaxAge
        {
            get { return maxAge; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void ProcessRequest (object resourceData, IWebContext context, Action<object, IWebContext> returnResourceAction)
        {
            DateTime? lastModified = lastModifiedFunc (resourceData);
            bool resourceModified = true;

            do
            {
                if (!lastModified.HasValue)
                    break;

                // strip milliseconds
                lastModified = new DateTime(
                    lastModified.Value.Year, lastModified.Value.Month, lastModified.Value.Day, lastModified.Value.Hour, lastModified.Value.Minute, lastModified.Value.Second);

                string ifModifiedSinceString = context.RequestHeaders[HttpConsts.HeaderIfModifiedSince];
                
                if (ifModifiedSinceString == null)
                    break;

                DateTime? ifModifiedSince = WebServerExtensions.FromRfc2822DateTime(ifModifiedSinceString);

                if (!ifModifiedSince.HasValue)
                    break;
                
                if (lastModified <= ifModifiedSince)
                    resourceModified = false;
            } 
            while (false);

            context.StatusCode = resourceModified ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotModified;

            context.RemoveHeader(HttpConsts.HeaderCacheControl);
            context.AddHeader(HttpConsts.HeaderCacheControl, HttpConsts.CacheControlPrivate);
            context.AddHeader(
                HttpConsts.HeaderCacheControl,
                "{0}={1}".Fmt(HttpConsts.CacheControlMaxAge, (int)maxAge.TotalSeconds));

            DateTime currentTime = context.TimeService.CurrentTime;
            context.AddHeader (HttpConsts.HeaderDate, currentTime.ToRfc2822DateTime ());
            context.AddHeader (HttpConsts.HeaderExpires, currentTime.Add (maxAge).ToRfc2822DateTime ());
            context.AddHeader (HttpConsts.HeaderAge, "0");

            if (lastModified.HasValue)
                context.AddHeader (HttpConsts.HeaderLastModified, lastModified.Value.ToRfc2822DateTime ());

            if (resourceModified)
                returnResourceAction(resourceData, context);

            if (!context.IsResponseClosed)
                context.CloseResponse();
        }

        private readonly TimeSpan maxAge;
        private readonly Func<object, DateTime?> lastModifiedFunc;
    }
}