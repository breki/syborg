using System;
using LibroLib.FileSystem;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.Caching;
using Syborg.CommandResults;
using Syborg.Common;

namespace Syborg.Tests.WebCommandResultTests
{
    public class CommandResultsTests
    {
        [Test] 
        public void BinaryDataResultShouldSetContentLength()
        {
            const int DataLength = 100;
            byte[] data = new byte[DataLength];
            
            BinaryDataResult result = new BinaryDataResult(data, new NoCachingPolicy());

            result.Apply(context);

            Assert.AreEqual (DataLength, context.ResponseContentLength);
        }

        [Test]
        public void TextResultShouldSetContentLength ()
        {
            TextResult result = new TextResult ("something");

            result.Apply (context);

            Assert.AreEqual (9, context.ResponseContentLength);
        }

        [SetUp]
        public void Setup()
        {
            fileSystem = MockRepository.GenerateStub<IFileSystem>();
            configuration = new WebServerConfiguration();

            ITimeService timeService = MockRepository.GenerateStub<ITimeService>();
            timeService.Stub(x => x.CurrentTime).Return(new DateTime(2014, 06, 16, 20, 07, 00));

            context = new FakeWebContext(null, null, fileSystem, timeService, configuration);
        }

        private FakeWebContext context;
        private IFileSystem fileSystem;
        private IWebServerConfiguration configuration;
    }
}