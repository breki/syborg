using System;
using System.Net;
using NUnit.Framework;
using Syborg.Caching;

namespace Syborg.Tests.HttpCachingTests
{
    public class CachingByETagPolicyTests : HttpCachingTestsBase
    {
        [Test]
        public void IfETagHeaderIsMissingReturnResource()
        {
            policy = new CachingByETagPolicy (maxAge, () => new Tuple<string, DateTime?>(null, lastModifiedTime));
            policy.ProcessRequest (null, WebContext, ReturnResource);
            
            Assert.AreEqual((int)HttpStatusCode.OK, WebContext.StatusCode);
            Assert.IsTrue(ResourceReturned);
        }

        [Test]
        public void IfNoneMatchHeaderIsMissingReturnResource()
        {
            policy = new CachingByETagPolicy (maxAge, () => new Tuple<string, DateTime?>(ETag, lastModifiedTime));
            policy.ProcessRequest (null, WebContext, ReturnResource);
            
            Assert.AreEqual((int)HttpStatusCode.OK, WebContext.StatusCode);
            Assert.IsTrue(ResourceReturned);
        }

        [Test]
        public void ETagAndIfNoneMatchHeaderAreNotMatching ()
        {
            WebContext.RequestHeaders.Add (HttpConsts.HeaderIfNoneMatch, "sdsdsdsd");

            policy = new CachingByETagPolicy (maxAge, () => new Tuple<string, DateTime?>(ETag, lastModifiedTime));
            policy.ProcessRequest (null, WebContext, ReturnResource);

            Assert.AreEqual ((int)HttpStatusCode.OK, WebContext.StatusCode);
            Assert.IsTrue (ResourceReturned);
        }

        [Test]
        public void ETagAndIfNoneMatchHeaderAreMatching ()
        {
            WebContext.RequestHeaders.Add (HttpConsts.HeaderIfNoneMatch, ETag);

            policy = new CachingByETagPolicy (maxAge, () => new Tuple<string, DateTime?>(ETag, lastModifiedTime));
            policy.ProcessRequest (null, WebContext, ReturnResource);

            Assert.AreEqual ((int)HttpStatusCode.NotModified, WebContext.StatusCode);
            Assert.IsFalse(ResourceReturned);
        }

        [TearDown]
        public void Teardown()
        {
            Assert.IsTrue (WebContext.IsResponseClosed);
        }

        private CachingByETagPolicy policy;
        private TimeSpan maxAge = TimeSpan.FromHours (1);
        private DateTime lastModifiedTime = new DateTime (2014, 06, 15, 18, 14, 00);
        private const string ETag = "\"12345\"";
    }
}