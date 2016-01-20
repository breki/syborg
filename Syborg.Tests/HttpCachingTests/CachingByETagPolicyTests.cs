using System;
using NUnit.Framework;
using Syborg.Caching;

namespace Syborg.Tests.HttpCachingTests
{
    public class CachingByETagPolicyTests : HttpCachingTestsBase
    {
        [Test]
        public void CacheByETagShouldCloseResponseEvenWhenFresh ()
        {
            TimeSpan maxAge = TimeSpan.FromHours (1);
            DateTime lastModifiedTime = new DateTime (2014, 06, 15, 18, 14, 00);
            const string ETag = "x";
            CachingByETagPolicy policy = new CachingByETagPolicy(maxAge, () => new Tuple<string, DateTime?>(ETag, lastModifiedTime));

            DateTime cachedTime = lastModifiedTime;
            WebContext.RequestHeaders.Add (HttpConsts.HeaderIfNoneMatch, ETag);

            policy.ProcessRequest (null, WebContext, ReturnResource);

            Assert.IsTrue (WebContext.IsResponseClosed);
        }
    }
}