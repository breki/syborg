using System;
using System.Globalization;
using System.Net;
using LibroLib.Misc;
using NUnit.Framework;
using Rhino.Mocks;

namespace Syborg.Tests.HttpCachingTests
{
    public abstract class HttpCachingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            timeService = MockRepository.GenerateStub<ITimeService>();
            currentTime = new DateTime(2014, 06, 15, 21, 55, 0);
            timeService.Stub(x => x.CurrentTime).Return(currentTime);

            webContext = new FakeWebContext (null, null, null, timeService, null);
            resourceReturned = false;
        }

        protected ITimeService TimeService
        {
            get { return timeService; }
        }

        protected FakeWebContext WebContext
        {
            get { return webContext; }
        }

        protected DateTime CurrentTime
        {
            get { return currentTime; }
        }

        protected bool ResourceReturned
        {
            get { return resourceReturned; }
        }

        protected void ReturnResource(object resourceData, IWebContext context)
        {
            resourceReturned = true;
        }

        protected void AssertCachedByAgeResult(HttpStatusCode httpStatusCode, TimeSpan maxAge, DateTime lastModifiedTime)
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