using System;
using System.Net;
using NUnit.Framework;
using Syborg.Caching;

namespace Syborg.Tests.HttpCachingTests
{
    public class CachingByMaxAgePolicyTests : HttpCachingTestsBase
    {
        [Test]
        public void CacheByMaxAgeWhenNew ()
        {
            TimeSpan maxAge = TimeSpan.FromHours (1);
            DateTime lastModifiedTime = new DateTime (2014, 06, 15, 18, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy (maxAge, x => lastModifiedTime);

            policy.ProcessRequest (null, WebContext, ReturnResource);

            AssertCachedByAgeResult (HttpStatusCode.OK, maxAge, lastModifiedTime);
            Assert.IsTrue (ResourceReturned);
        }

        [Test]
        public void SubsecondsShouldBeIgnoredWhenComparingTimes ()
        {
            TimeSpan maxAge = TimeSpan.FromHours (1);
            DateTime lastModifiedTime = new DateTime (2014, 06, 15, 18, 14, 00, 666);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy (maxAge, x => lastModifiedTime);

            DateTime cachedTime = lastModifiedTime;
            WebContext.RequestHeaders.Add (HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime ());

            policy.ProcessRequest (null, WebContext, ReturnResource);

            AssertCachedByAgeResult (HttpStatusCode.NotModified, maxAge, lastModifiedTime);
            Assert.IsFalse (ResourceReturned);
        }

        [Test]
        public void CacheByMaxAgeWhenFresh ()
        {
            TimeSpan maxAge = TimeSpan.FromHours (1);
            DateTime lastModifiedTime = new DateTime (2014, 06, 15, 18, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy (maxAge, x => lastModifiedTime);

            DateTime cachedTime = lastModifiedTime;
            WebContext.RequestHeaders.Add (HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime ());

            policy.ProcessRequest (null, WebContext, ReturnResource);

            AssertCachedByAgeResult (HttpStatusCode.NotModified, maxAge, lastModifiedTime);
            Assert.IsFalse (ResourceReturned);
        }

        [Test]
        public void CacheByMaxAgeShouldCloseResponseEvenWhenFresh ()
        {
            TimeSpan maxAge = TimeSpan.FromHours (1);
            DateTime lastModifiedTime = new DateTime (2014, 06, 15, 18, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy (maxAge, x => lastModifiedTime);

            DateTime cachedTime = lastModifiedTime;
            WebContext.RequestHeaders.Add (HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime ());

            policy.ProcessRequest (null, WebContext, ReturnResource);

            Assert.IsTrue (WebContext.IsResponseClosed);
        }

        [Test]
        public void CacheByMaxAgeWhenStale ()
        {
            TimeSpan maxAge = TimeSpan.FromHours (1);
            DateTime lastModifiedTime = new DateTime (2014, 06, 15, 20, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy (maxAge, x => lastModifiedTime);

            DateTime cachedTime = new DateTime (2014, 06, 15, 18, 14, 00);
            WebContext.RequestHeaders.Add (HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime ());

            policy.ProcessRequest (null, WebContext, ReturnResource);

            AssertCachedByAgeResult (HttpStatusCode.OK, maxAge, lastModifiedTime);
            Assert.IsTrue (ResourceReturned);
        }
    }
}