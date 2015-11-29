using System;
using System.Net;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.CommandResults;
using Syborg.Routing;

namespace Syborg.Tests
{
    public class RouteMatchingTests
    {
        [Test]
        public void RoutesMustBeUrlDecoded()
        {
            IWebCommand command = MockRepository.GenerateMock<IWebCommand>();
            command.Expect(x => x.Execute(null, null)).IgnoreArguments()
                   .Do(new Func<IWebContext, WebRequestRouteMatch, IWebCommandResult>(
                           (c, r) =>
                               {
                                   Assert.AreEqual ("côte-d'ivoire", r["PlaceId"]);
                                   return new HttpStatusResult(HttpStatusCode.Accepted);
                               }));

            SetupRoutingTest ("^place/(?<PlaceId>.+)$", "/place/côte-d'ivoire", "GET", true, command);
        }

        [Test]
        public void WhenRouteIsMatchingExecuteCommand()
        {
            SetupRoutingTest("^test-route$", "/test-route", "GET", true);
        }

        [Test]
        public void MethodShouldMatchToo()
        {
            SetupRoutingTest ("^test-route$", "/test-route", "POST", false);
        }

        [Test]
        public void TrailingSlashShouldBeIgnored()
        {
            SetupRoutingTest ("^test-route$", "/test-route/", "GET", true);
        }

        [Test]
        public void RouteWithQueryParameters()
        {
            SetupRoutingTest ("^test-route$", "/test-route?a=2", "GET", true);
        }

        [Test]
        public void HandleMinimalRoute()
        {
            SetupRoutingTest (".*", "/", "GET", true);
        }

        [Test]
        public void ServerWithUrlSuffixThatMatches()
        {
            webContext.Stub(x => x.ApplicationPath).Return("/path1/path2");
            SetupRoutingTest ("^something$", "/path1/path2/something", "GET", true);
        }

        [Test]
        public void ServerWithUrlSuffixThatDoesNotMatch()
        {
            webContext.Stub (x => x.ApplicationPath).Return ("/path1/path2");
            SetupRoutingTest ("^something$", "/something", "GET", false);
        }

        [Test]
        public void ApplicationPathShouldBeCaseInsensitive()
        {
            webContext.Stub (x => x.ApplicationPath).Return ("/Path1/Path2");
            SetupRoutingTest ("^something$", "/path1/Path2/something", "GET", true);
        }

        [Test]
        public void ApplicationPathIsCaseInsensitive()
        {
            webContext.Stub (x => x.ApplicationPath).Return ("/WagaHaga");
            SetupRoutingTest ("^home$", "/wagahaga/home", "GET", true);
        }

        [Test]
        public void EmptyRoutePath()
        {
            webContext.Stub (x => x.ApplicationPath).Return ("/WagaHaga");
            SetupRoutingTest ("^$", "/wagahaga", "GET", true);
        }

        [SetUp]
        public void Setup()
        {
            webContext = MockRepository.GenerateStub<IWebContext>();
            webCommand = MockRepository.GenerateMock<IWebCommand>();
            configuration = new WebServerConfiguration ();
        }

        private void SetupRoutingTest(string routePattern, string rawUrl, string method, bool expectMatch, IWebCommand overrideCommand = null)
        {
            route = new RegexWebRequestRoute (routePattern, "GET", overrideCommand ?? webCommand);

            Uri url = new Uri("http://azuritemaps.com" + rawUrl);
            webContext.Stub (x => x.Url).Return (url);
            webContext.Stub (x => x.RawUrl).Return (rawUrl);
            webContext.Stub (x => x.HttpMethod).Return (method);

            if (expectMatch && overrideCommand == null)
            {
                webCommand.Stub(x => x.Execute(null, null))
                    .IgnoreArguments()
                    .Return(new HttpStatusResult(HttpStatusCode.Accepted));
            }

            IWebCommandResult expectedCommandResult;

            Assert.AreEqual(expectMatch, route.ProcessIfMatch (webContext, out expectedCommandResult));

            if (expectMatch)
            {
                Assert.IsInstanceOf<HttpStatusResult>(expectedCommandResult);
                Assert.AreEqual((int)HttpStatusCode.Accepted, ((HttpStatusResult)expectedCommandResult).StatusCode);
            }
        }

        private IWebRequestRoute route;
        private IWebCommand webCommand;
        private IWebContext webContext;
        private WebServerConfiguration configuration;
    }
}