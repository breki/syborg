﻿using System;
using System.Diagnostics.Contracts;
using System.Net;
using LibroLib;

namespace Syborg.Caching
{
    /// <summary>
    /// Caching policy that uses ETag request header to determine whether the cached resource is stale.
    /// </summary>
    public class CachingByETagPolicy : ICachingPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachingByETagPolicy"/> class using the specified maxAge value
        /// and the function to return ETag value and the lastModified DateTime of the resource.
        /// </summary>
        /// <param name="maxAge">The value to be used for the Cache-Control max-age HTTP response header</param>
        /// <param name="etagFunc">The function that returns the ETag and Last-Modified HTTP response header values</param>
        public CachingByETagPolicy (TimeSpan maxAge, Func<Tuple<string, DateTime?>> etagFunc)
        {
            Contract.Requires(etagFunc != null);

            this.maxAge = maxAge;
            this.etagFunc = etagFunc;
        }

        public TimeSpan MaxAge
        {
            get { return maxAge; }
        }

        public Func<Tuple<string, DateTime?>> EtagFunc
        {
            get { return etagFunc; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void ProcessRequest (object resourceData, IWebContext context, Action<object, IWebContext> returnResourceAction)
        {
            Tuple<string, DateTime?> tuple = etagFunc();
            string etag = tuple.Item1;
            DateTime? lastModified = tuple.Item2;

            bool resourceModified = true;

            do
            {
                if (etag == null)
                    break;

                string ifNoneMatch = context.RequestHeaders[HttpConsts.HeaderIfNoneMatch];

                if (ifNoneMatch == null)
                    break;

                if (string.Compare(etag, ifNoneMatch, StringComparison.Ordinal) == 0)
                    resourceModified = false;
            }
            while (false);

            context.StatusCode = resourceModified ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotModified;

            context.RemoveHeader(HttpConsts.HeaderCacheControl);
            context.AddHeader (HttpConsts.HeaderCacheControl, HttpConsts.CacheControlPrivate);
            context.AddHeader (
                HttpConsts.HeaderCacheControl,
                "{0}={1}".Fmt (HttpConsts.CacheControlMaxAge, (int)maxAge.TotalSeconds));

            if (etag != null)
                context.AddHeader(HttpConsts.HeaderETag, etag);

            DateTime currentTime = context.TimeService.CurrentTime;
            context.AddHeader (HttpConsts.HeaderDate, currentTime.ToRfc2822DateTime ());
            context.AddHeader (HttpConsts.HeaderExpires, currentTime.Add (maxAge).ToRfc2822DateTime ());
            context.AddHeader (HttpConsts.HeaderAge, "0");

            if (lastModified.HasValue)
                context.AddHeader (HttpConsts.HeaderLastModified, lastModified.Value.ToRfc2822DateTime ());

            if (resourceModified)
                returnResourceAction (resourceData, context);

            if (!context.IsResponseClosed)
                context.CloseResponse ();
        }

        private readonly TimeSpan maxAge;
        private readonly Func<Tuple<string, DateTime?>> etagFunc;
    }
}