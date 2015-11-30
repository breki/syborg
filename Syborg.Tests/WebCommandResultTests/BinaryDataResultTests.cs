using System;
using System.Net;
using LibroLib.FileSystem;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.Caching;
using Syborg.CommandResults;
using Syborg.Common;

namespace Syborg.Tests.WebCommandResultTests
{
    public class BinaryDataResultTests
    {
        [Test]
        public void WithCachingPolicyAndCacheVersionIsValid ()
        {
            const string ETag = "\"1\"";

            BinaryDataResult result = new BinaryDataResult (
                new byte[] { 0, 1, 2 }, 
                new CachingByETagPolicy (TimeSpan.FromDays (1), () => new Tuple<string, DateTime?>(ETag, timeService.CurrentTime)));
            result.StatusCode = (int)HttpStatusCode.Accepted;
            context.RequestHeaders[HttpConsts.HeaderIfNoneMatch] = ETag;

            result.Apply (context);
            Assert.AreEqual ((int)HttpStatusCode.NotModified, context.StatusCode);
        }

        [SetUp]
        public void Setup ()
        {
            fileSystem = MockRepository.GenerateStub<IFileSystem>();
            timeService = new TimeMachine ();
            IWebServerConfiguration configuration = MockRepository.GenerateStub<IWebServerConfiguration>();
            configuration.Stub (x => x.WebServerDevelopmentMode).Return (false);
            context = new FakeWebContext (null, null, fileSystem, timeService, configuration);
        }

        private FakeWebContext context;
        private IFileSystem fileSystem;
        private TimeMachine timeService;
    }
}