using System.Linq;
using System.Net;
using LibroLib.WebUtils;
using LibroLib.WebUtils.Rest;
using NUnit.Framework;

namespace Syborg.Tests.HttpModuleTests
{
    public class BasicHttpModuleTests
    {
        [Test]
        public void ServerHeaderShouldNotBeEmitted()
        {
            using (IRestClient client = restClientFactory.CreateRestClient ())
            {
                IRestClientResponse response = client.Get (WebAppUrl + "content/cached-by-max-age.txt").Do ().Response;
                Assert.AreEqual ((int)HttpStatusCode.OK, client.StatusCode);
                Assert.IsFalse(response.Headers.AllKeys.Any(x => x == HttpConsts.HeaderServer));
            }
        }

        [SetUp]
        public void Setup()
        {
            IWebConfiguration webConfiguration = new WebConfiguration ("Syborg.Tests");
            restClientFactory = new RestClientFactory (webConfiguration);            
        }

        private RestClientFactory restClientFactory;
        private const string WebAppUrl = "http://localhost/syborg-tests/";
    }
}