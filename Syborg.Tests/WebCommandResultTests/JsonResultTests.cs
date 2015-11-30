using System.Collections.Generic;
using System.Net;
using System.Text;
using LibroLib.FileSystem;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.CommandResults;
using Syborg.Common;

namespace Syborg.Tests.WebCommandResultTests
{
    public class JsonResultTests
    {
        [Test]
        public void JsonShouldNotEmitBom()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("key", "value");

            JsonResult result = new JsonResult(dict);
            result.Apply(context);

            byte[] responseBytes = context.ReadResponseStreamAsByteArray();
            string responseBody = Encoding.UTF8.GetString(responseBytes);

            Assert.AreNotEqual(239, responseBytes[0]);
            Assert.AreEqual("{\"key\":\"value\"}", responseBody);
        }

        [Test]
        public void JsonResultShouldSetContentLength ()
        {
            JsonResult result = new JsonResult (HttpStatusCode.OK, "something");

            result.Apply (context);

            Assert.AreEqual (11, context.ResponseContentLength);
        }

        [SetUp]
        public void Setup ()
        {
            fileSystem = MockRepository.GenerateStub<IFileSystem>();
            ITimeService timeService = MockRepository.GenerateStub<ITimeService>();
            IWebServerConfiguration configuration = MockRepository.GenerateStub<IWebServerConfiguration>();
            configuration.Stub (x => x.WebServerDevelopmentMode).Return (false);
            context = new FakeWebContext (null, null, fileSystem, timeService, configuration);
        }

        private FakeWebContext context;
        private IFileSystem fileSystem;
    }
}