using System.Net;
using LibroLib.WebUtils.Rest;
using NUnit.Framework;

namespace Syborg.Tests.HttpModuleTests
{
    public class StreamResultIntegrationTests : HttpModuleTestsBase
    {
        [Test]
        public void CheckShortTextResponse()
        {
            using (IRestClient client = RestClientFactory.CreateRestClient ())
            {
                IRestClientResponse response = client.Get (WebAppUrl + "stream/short-text").Do ().Response;
                Assert.AreEqual ((int)HttpStatusCode.OK, client.StatusCode);
                Assert.AreEqual("This is a stream response", response.AsString());
            }
        }

        [Test]
        public void CheckLongBinaryResponse()
        {
            using (IRestClient client = RestClientFactory.CreateRestClient ())
            {
                IRestClientResponse response = client.Get (WebAppUrl + "stream/long-binary").Do ().Response;
                Assert.AreEqual ((int)HttpStatusCode.OK, client.StatusCode);
                byte[] bytes = response.AsBytes();
                Assert.AreEqual(4 * 1024 * 1024, bytes.Length);
            }
        }
    }
}