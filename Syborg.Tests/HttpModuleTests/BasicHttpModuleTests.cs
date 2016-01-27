using System.Linq;
using System.Net;
using LibroLib.WebUtils.Rest;
using NUnit.Framework;

namespace Syborg.Tests.HttpModuleTests
{
    public class BasicHttpModuleTests : HttpModuleTestsBase
    {
        [Test]
        public void ServerHeaderShouldNotBeEmitted()
        {
            using (IRestClient client = RestClientFactory.CreateRestClient ())
            {
                IRestClientResponse response = client.Get (WebAppUrl + "content/cached-by-max-age.txt").Do ().Response;
                Assert.AreEqual ((int)HttpStatusCode.OK, client.StatusCode);
                Assert.IsFalse(response.Headers.AllKeys.Any(x => x == HttpConsts.HeaderServer));
            }
        }
    }
}