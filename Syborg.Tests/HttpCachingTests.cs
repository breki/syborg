using System;
using System.Globalization;
using System.Net;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.Caching;
using Syborg.Common;

namespace Syborg.Tests
{
    public class HttpCachingTests
    {
        [Test]
        public void NoStore()
        {
            NoCachingPolicy policy = new NoCachingPolicy();
            policy.ProcessRequest(null, webContext, ReturnResource);
            Assert.AreEqual("no-cache,no-store,max-age=0", webContext.ResponseHeaders[HttpConsts.HeaderCacheControl]);
            Assert.AreEqual(currentTime.ToRfc2822DateTime(), webContext.ResponseHeaders[HttpConsts.HeaderDate]);
            Assert.AreEqual(new DateTime(2000, 01, 01).ToRfc2822DateTime(), webContext.ResponseHeaders[HttpConsts.HeaderExpires]);
            Assert.IsTrue(resourceReturned);
        }

        [Test]
        public void CacheByMaxAgeWhenNew()
        {
            TimeSpan maxAge = TimeSpan.FromHours(1);
            DateTime lastModifiedTime = new DateTime(2014, 06, 15, 18, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy(maxAge, x => lastModifiedTime);

            policy.ProcessRequest (null, webContext, ReturnResource);

            AssertCachedByAgeResult(HttpStatusCode.OK, maxAge, lastModifiedTime);
            Assert.IsTrue (resourceReturned);
        }

        [Test]
        public void SubsecondsShouldBeIgnoredWhenComparingTimes()
        {
            TimeSpan maxAge = TimeSpan.FromHours(1);
            DateTime lastModifiedTime = new DateTime(2014, 06, 15, 18, 14, 00, 666);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy(maxAge, x => lastModifiedTime);

            DateTime cachedTime = lastModifiedTime;
            webContext.RequestHeaders.Add(HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime());

            policy.ProcessRequest (null, webContext, ReturnResource);

            AssertCachedByAgeResult (HttpStatusCode.NotModified, maxAge, lastModifiedTime);
            Assert.IsFalse(resourceReturned);
        }

        [Test]
        public void CacheByMaxAgeWhenFresh()
        {
            TimeSpan maxAge = TimeSpan.FromHours(1);
            DateTime lastModifiedTime = new DateTime(2014, 06, 15, 18, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy(maxAge, x => lastModifiedTime);

            DateTime cachedTime = lastModifiedTime;
            webContext.RequestHeaders.Add(HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime());

            policy.ProcessRequest (null, webContext, ReturnResource);

            AssertCachedByAgeResult (HttpStatusCode.NotModified, maxAge, lastModifiedTime);
            Assert.IsFalse(resourceReturned);
        }

        [Test]
        public void CacheByMaxAgeShouldCloseResponseEvenWhenFresh()
        {
            TimeSpan maxAge = TimeSpan.FromHours(1);
            DateTime lastModifiedTime = new DateTime(2014, 06, 15, 18, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy(maxAge, x => lastModifiedTime);

            DateTime cachedTime = lastModifiedTime;
            webContext.RequestHeaders.Add(HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime());

            policy.ProcessRequest (null, webContext, ReturnResource);

            Assert.IsTrue(webContext.IsResponseClosed);
        }

        [Test]
        public void CacheByMaxAgeWhenStale()
        {
            TimeSpan maxAge = TimeSpan.FromHours(1);
            DateTime lastModifiedTime = new DateTime(2014, 06, 15, 20, 14, 00);
            CachingByMaxAgePolicy policy = new CachingByMaxAgePolicy(maxAge, x => lastModifiedTime);

            DateTime cachedTime = new DateTime(2014, 06, 15, 18, 14, 00);
            webContext.RequestHeaders.Add(HttpConsts.HeaderIfModifiedSince, cachedTime.ToRfc2822DateTime());

            policy.ProcessRequest (null, webContext, ReturnResource);

            AssertCachedByAgeResult (HttpStatusCode.OK, maxAge, lastModifiedTime);
            Assert.IsTrue (resourceReturned);
        }

        [Test]
        public void CacheByETagShouldCloseResponseEvenWhenFresh ()
        {
            TimeSpan maxAge = TimeSpan.FromHours (1);
            DateTime lastModifiedTime = new DateTime (2014, 06, 15, 18, 14, 00);
            const string ETag = "x";
            CachingByETagPolicy policy = new CachingByETagPolicy(maxAge, () => new Tuple<string, DateTime?>(ETag, lastModifiedTime));

            DateTime cachedTime = lastModifiedTime;
            webContext.RequestHeaders.Add (HttpConsts.HeaderIfNoneMatch, ETag);

            policy.ProcessRequest (null, webContext, ReturnResource);

            Assert.IsTrue (webContext.IsResponseClosed);
        }

        [SetUp]
        public void Setup()
        {
            timeService = MockRepository.GenerateStub<ITimeService>();
            currentTime = new DateTime(2014, 06, 15, 21, 55, 0);
            timeService.Stub(x => x.CurrentTime).Return(currentTime);

            webContext = new FakeWebContext (null, null, null, timeService, null);
            resourceReturned = false;
        }

        private void ReturnResource(object resourceData, IWebContext context)
        {
            resourceReturned = true;
        }

        private void AssertCachedByAgeResult(HttpStatusCode httpStatusCode, TimeSpan maxAge, DateTime lastModifiedTime)
        {
            Assert.AreEqual ((int)httpStatusCode, webContext.StatusCode);

            Assert.AreEqual (
                "private,max-age=" + maxAge.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                webContext.ResponseHeaders[HttpConsts.HeaderCacheControl]);
            Assert.AreEqual(lastModifiedTime.ToRfc2822DateTime(), webContext.ResponseHeaders[HttpConsts.HeaderLastModified]);
            Assert.AreEqual(currentTime.ToRfc2822DateTime(), webContext.ResponseHeaders[HttpConsts.HeaderDate]);
            Assert.AreEqual(currentTime.Add(maxAge).ToRfc2822DateTime(), webContext.ResponseHeaders[HttpConsts.HeaderExpires]);

            Assert.AreEqual("0", webContext.ResponseHeaders[HttpConsts.HeaderAge]);
        }
        
        private ITimeService timeService;
        private FakeWebContext webContext;
        private DateTime currentTime;
        private bool resourceReturned;
    }
}