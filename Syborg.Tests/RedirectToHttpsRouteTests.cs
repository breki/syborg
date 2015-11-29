using System;
using System.Net;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.CommandResults;
using Syborg.Common;
using Syborg.Routing;

namespace Syborg.Tests
{
    public class RedirectToHttpsRouteTests
    {
        [TestCase (HttpsMode.RequireHttps, null)]
        [TestCase (HttpsMode.RequireHttps, 12345)]
        [TestCase (HttpsMode.RequireHttpsExceptLocal, null)]
        [TestCase (HttpsMode.RequireHttpsExceptLocal, 12345)]
        public void IfHttpsIsRequiredAndRequestIsInsecureThenRedirectIt (HttpsMode mode, int? httpsPort)
        {
            configuration.Stub(x => x.HttpsMode).Return(mode);
            configuration.Stub(x => x.HttpsPort).Return(httpsPort);
            context = new FakeWebContext ("http://localhost", null, null, null, configuration);
            context.Url = new Uri ("http://wagahaga.com/map/new-york");

            IWebCommandResult result;
            Assert.IsTrue(route.ProcessIfMatch(context, out result));

            HttpStatusResult statusResult = (HttpStatusResult)result;
            Assert.AreEqual((int)HttpStatusCode.MovedPermanently, statusResult.StatusCode);

            if (httpsPort.HasValue)
                Assert.AreEqual ("https://wagahaga.com:{0}/map/new-york".Fmt (httpsPort), statusResult.Headers[HttpConsts.HeaderLocation]);
            else
                Assert.AreEqual ("https://wagahaga.com/map/new-york", statusResult.Headers[HttpConsts.HeaderLocation]);
        }

        [Test]
        public void IfHttpsIsRequiredAndRequestIsSecureThenThisRouteDoesNotMatch()
        {
            configuration.Stub(x => x.HttpsMode).Return(HttpsMode.RequireHttps);
            context.IsSecureConnection = true;

            IWebCommandResult result;
            Assert.IsFalse(route.ProcessIfMatch(context, out result));
        }

        [Test]
        public void IfHttpsIsNotRequiredForLocalRequests()
        {
            configuration.Stub (x => x.HttpsMode).Return (HttpsMode.RequireHttpsExceptLocal);
            context.IsRequestLocal = true;

            IWebCommandResult result;
            Assert.IsFalse (route.ProcessIfMatch (context, out result));
        }

        [Test]
        public void IfHttpsIsNotRequiredThisRouteDoesNotMatch()
        {
            configuration.Stub (x => x.HttpsMode).Return (HttpsMode.AllowBoth);
            IWebCommandResult result;
            Assert.IsFalse(route.ProcessIfMatch (context, out result));
        }

        [SetUp]
        public void Setup()
        {
            configuration = MockRepository.GenerateStub<IWebServerConfiguration>();
            context = new FakeWebContext ("http://localhost", null, null, null, configuration);
            route = new RedirectToHttpsRoute();
        }

        private RedirectToHttpsRoute route;
        private FakeWebContext context;
        private IWebServerConfiguration configuration;
    }
}