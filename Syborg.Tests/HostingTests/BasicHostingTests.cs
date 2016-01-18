using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using LibroLib;
using LibroLib.FileSystem;
using LibroLib.Misc;
using LibroLib.Threading;
using LibroLib.WebUtils;
using LibroLib.WebUtils.Rest;
using NUnit.Framework;
using Syborg.Commands;
using Syborg.ContentHandling;
using Syborg.Razor;
using Syborg.Routing;

namespace Syborg.Tests.HostingTests
{
    [Category("integration")]
    public class BasicHostingTests
    {
        [Test]
        public void GZipCompressDecompress()
        {
            string text = "this is a text";
            byte[] textData = Encoding.UTF8.GetBytes(text);

            byte[] compressedData;
            using (MemoryStream compressedStream = new MemoryStream ())
            {
                using (GZipStream zipStream = new GZipStream (compressedStream, CompressionMode.Compress))
                    zipStream.Write (textData, 0, textData.Length);

                compressedData = compressedStream.ToArray ();
            }

            byte[] decompressedData;
            using (MemoryStream compressedStream = new MemoryStream(compressedData, false))
            {
                using (GZipStream zipStream = new GZipStream (compressedStream, CompressionMode.Decompress))
                {
                    using (MemoryStream decompressedStream = new MemoryStream ())
                    {
                        zipStream.CopyTo (decompressedStream);
                        decompressedStream.Flush ();
                        decompressedData = decompressedStream.ToArray ();
                    }
                }
            }

            CollectionAssert.AreEqual(decompressedData, textData);
        }

        [Test]
        public void FetchContentFileWithoutCompression()
        {
            using (IRestClient client = restClientFactory.CreateRestClient())
            {
                IRestClientResponse response = client.Get(testServiceUrl + "content/sample.txt").Do().Response;
                Assert.AreEqual((int)HttpStatusCode.OK, client.StatusCode);
                Assert.AreEqual ("This is sample content.", response.AsString());
            }
        }

        [Test]
        public void FetchContentFileWithCompression()
        {
            using (IRestClient client = restClientFactory.CreateRestClient())
            {
                IRestClientResponse response = client.Get(testServiceUrl + "content/sample.txt")
                    .AddHeader(HttpRequestHeader.AcceptEncoding, "gzip")
                    .Do().Response;
                Assert.AreEqual((int)HttpStatusCode.OK, client.StatusCode);
                byte[] compressedData = response.AsBytes();

                using (MemoryStream stream = new MemoryStream(compressedData))
                using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
                using (MemoryStream resultStream = new MemoryStream())
                {
                    zipStream.CopyTo(resultStream);
                    byte[] uncompressedData = resultStream.ToArray();
                    Assert.AreEqual ("This is sample content.", new UTF8Encoding(false).GetString(uncompressedData));
                }
            }
        }

        [OneTimeSetUp]
        public void Setup()
        {
            IFileSystem fileSystem = new WindowsFileSystem();
            IApplicationInfo applicationInfo = new ApplicationInfo();
            ITimeService timeService = new RealTimeService ();

            ISignal serverStopSignal = new ManualResetSignal (false);

            IWebServerConfiguration configuration = new WebServerConfiguration();

            IRazorCompiler razorCompiler = new InMemoryRazorCompiler();
            IRazorViewRenderingEngine viewRenderingEngine = new RazorViewRenderingEngine(fileSystem, razorCompiler);

            IWebServerController webServerController = new WebServerController (serverStopSignal);
            IFileMimeTypesMap fileMimeTypesMap = new FileMimeTypesMap ();
            IFileCache fileCache = new FileCache ();

            List<IWebRequestRoute> routes = new List<IWebRequestRoute>();
            string contentRootDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "sample-content");
            ContentCommand contentCommand = new ContentCommand(contentRootDirectory, fileSystem, fileCache);
            routes.Add (new RegexWebRequestRoute ("^content/(?<path>.+)$", HttpMethod.GET, contentCommand));

            List<IWebPolicy> policies = new List<IWebPolicy>();

            const string ExternalUrl = "http://localhost";
            const int Port = 12345;
            testServiceUrl = "{0}:{1}/".Fmt(ExternalUrl, Port);

            host = new TestHost(
                configuration, ExternalUrl, Port, null, fileSystem, applicationInfo, timeService, viewRenderingEngine, fileMimeTypesMap, webServerController, routes, policies);
            host.Start ();

            IWebConfiguration webConfiguration = new WebConfiguration("Syborg.Tests");
            restClientFactory = new RestClientFactory(webConfiguration);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            if (host != null)
            {
                host.Stop();
                host.Dispose();
                host = null;
            }
        }

        private TestHost host;
        private IRestClientFactory restClientFactory;
        private string testServiceUrl;
    }
}
