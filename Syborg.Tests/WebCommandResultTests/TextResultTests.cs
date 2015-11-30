using System;
using System.Net;
using LibroLib.Misc;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.Caching;
using Syborg.CommandResults;
using Syborg.Common;

namespace Syborg.Tests.WebCommandResultTests
{
    public class TextResultTests
    {
        [Test]
        public void DefaultBehavior()
        {
            TextResult result = new TextResult("text");
            result.Apply (context);
            Assert.AreEqual ((int)HttpStatusCode.OK, context.StatusCode);
            Assert.AreEqual (HttpConsts.ContentTypeTextPlainUtf8, context.ResponseContentType);
            Assert.AreEqual ("text", context.ReadResponseStreamAsText());
        }

        [Test]
        public void CustomStatusCode()
        {
            TextResult result = new TextResult("text", HttpStatusCode.Accepted);
            result.Apply (context);
            Assert.AreEqual ((int)HttpStatusCode.Accepted, context.StatusCode);
            Assert.AreEqual (HttpConsts.ContentTypeTextPlainUtf8, context.ResponseContentType);
            Assert.AreEqual ("text", context.ReadResponseStreamAsText());
        }

        [Test]
        public void CustomContentType()
        {
            TextResult result = new TextResult("text", HttpStatusCode.Accepted);
            result.ContentType = HttpConsts.ContentTypeApplicationJson;
            result.Apply (context);
            Assert.AreEqual ((int)HttpStatusCode.Accepted, context.StatusCode);
            Assert.AreEqual (HttpConsts.ContentTypeApplicationJson, context.ResponseContentType);
            Assert.AreEqual ("text", context.ReadResponseStreamAsText());
        }

        [Test]
        public void WithCachingPolicyAndCacheVersionIsValid()
        {
            const string ETag = "\"1\"";

            TextResult result = new TextResult ("text", HttpStatusCode.Accepted);
            result.CachingPolicy = new CachingByETagPolicy(TimeSpan.FromDays(1), () => new Tuple<string, DateTime?>(ETag, timeService.CurrentTime));
            result.ContentType = HttpConsts.ContentTypeApplicationJson;

            context.RequestHeaders[HttpConsts.HeaderIfNoneMatch] = ETag;

            result.Apply (context);
            Assert.AreEqual ((int)HttpStatusCode.NotModified, context.StatusCode);
        }

        [SetUp]
        public void Setup ()
        {
            timeService = new TimeMachine();
            IWebServerConfiguration configuration = MockRepository.GenerateStub<IWebServerConfiguration>();
            configuration.Stub (x => x.WebServerDevelopmentMode).Return (false);
            context = new FakeWebContext (null, null, null, timeService, configuration);
        }

        private FakeWebContext context;
        private ITimeService timeService;
    }
}