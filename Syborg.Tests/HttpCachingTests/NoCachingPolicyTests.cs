using System;
using NUnit.Framework;
using Syborg.Caching;

namespace Syborg.Tests.HttpCachingTests
{
    public class NoCachingPolicyTests : HttpCachingTestsBase
    {
        [Test]
        public void NoStore ()
        {
            NoCachingPolicy policy = new NoCachingPolicy ();
            policy.ProcessRequest (null, WebContext, ReturnResource);
            Assert.AreEqual ("no-cache,no-store,max-age=0", WebContext.ResponseHeaders[HttpConsts.HeaderCacheControl]);
            Assert.AreEqual (CurrentTime.ToRfc2822DateTime (), WebContext.ResponseHeaders[HttpConsts.HeaderDate]);
            Assert.AreEqual (new DateTime (2000, 01, 01).ToRfc2822DateTime (), WebContext.ResponseHeaders[HttpConsts.HeaderExpires]);
            Assert.IsTrue (ResourceReturned);
        }
    }
}