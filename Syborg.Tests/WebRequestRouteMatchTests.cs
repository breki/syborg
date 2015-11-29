using System.Text.RegularExpressions;
using NUnit.Framework;
using Syborg.Routing;

namespace Syborg.Tests
{
    public class WebRequestRouteMatchTests
    {
        [Test]
        public void ConstructFromRegexMatch()
        {
            string value = "tile/slovenia/123";

            Match match = pattern.Match(value);
            WebRequestRouteMatch routeMatch = WebRequestRouteMatch.FromRegexMatch(pattern, match);

            Assert.AreEqual("slovenia", routeMatch["layer"]);
            Assert.AreEqual("123", routeMatch["tileid"]);
        }

        [Test]
        public void IfParameterIsMissingReturnNull()
        {
            string value = "tile/slovenia/123";

            Match match = pattern.Match (value);
            WebRequestRouteMatch routeMatch = WebRequestRouteMatch.FromRegexMatch (pattern, match);

            Assert.IsNull(routeMatch["map"]);
        }

        [Test]
        public void ParameterIsOptional()
        {
            pattern = new Regex ("^tile(/(?<layer>[^/]+))?$", RegexOptions.Compiled);
            string value = "tile";

            Match match = pattern.Match (value);
            WebRequestRouteMatch routeMatch = WebRequestRouteMatch.FromRegexMatch (pattern, match);

            Assert.IsNull (routeMatch["layer"]);
        }

        [SetUp]
        public void Setup()
        {
            pattern = new Regex ("^tile/(?<layer>[^/]+)/(?<tileid>.+)$", RegexOptions.Compiled);
        }

        private Regex pattern;
    }
}