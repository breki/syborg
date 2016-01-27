using System.Net;
using LibroLib.WebUtils.Rest;
using NUnit.Framework;

namespace Syborg.Tests.HttpModuleTests
{
    public class StreamResultIntegrationTests : HttpModuleTestsBase
    {
        [Test]
        public void Test()
        {
            using (IRestClient client = RestClientFactory.CreateRestClient ())
            {
                IRestClientResponse response = client.Get (WebAppUrl + "stream/stream-result").Do ().Response;
                Assert.AreEqual ((int)HttpStatusCode.OK, client.StatusCode);
                Assert.AreEqual("This is a stream response", response.AsString());
            }
        }
    }
}