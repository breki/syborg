using System;
using System.Net;
using LibroLib.FileSystem;
using LibroLib.Misc;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.Caching;
using Syborg.CommandResults;

namespace Syborg.Tests.WebCommandResultTests
{
    public class FileResultTests
    {
        [Test]
        public void ReturnFile()
        {
            fileSystem.Stub (x => x.DoesFileExist (FileName)).Return (true);
            byte[] fileBytes = new byte[100];
            fileSystem.Stub(x => x.ReadFileAsBytes(FileName)).Return(fileBytes);

            ICachingPolicy cachingPolicy = new NoCachingPolicy();
            FileResult result = new FileResult(FileName, cachingPolicy);
            result.Apply(context);

            Assert.AreEqual((int)HttpStatusCode.OK, context.StatusCode);
            Assert.AreEqual(HttpConsts.ContentTypeImagePng, context.ResponseContentType);
            Assert.AreEqual (null, context.ResponseHeaders[HttpConsts.HeaderTransferEncoding]);
        }

        [Test]
        public void FileDoesNotExist()
        {
            fileSystem.Stub(x => x.DoesFileExist(FileName)).Return(false);

            ICachingPolicy cachingPolicy = new NoCachingPolicy ();
            FileResult result = new FileResult (FileName, cachingPolicy);
            result.Apply (context);

            Assert.AreEqual ((int)HttpStatusCode.NotFound, context.StatusCode);
        }

        [Test]
        public void FileResultShouldSetContentLength ()
        {
            const int DataLength = 100;
            byte[] data = new byte[DataLength];

            fileSystem.Stub (x => x.DoesFileExist (FileName)).Return (true);
            fileSystem.Stub (x => x.ReadFileAsBytes (FileName)).Return (data);

            FileResult result = new FileResult (FileName, new NoCachingPolicy ());

            result.Apply (context);

            Assert.AreEqual (DataLength, context.ResponseContentLength);
        }

        [Test]
        public void ContentTypeCouldNotBeDetermined()
        {
            fileSystem.Stub (x => x.DoesFileExist (FileNameStrangeExtension)).Return (true);
            byte[] fileBytes = new byte[100];
            fileSystem.Stub (x => x.ReadFileAsBytes (FileNameStrangeExtension)).Return (fileBytes);

            ICachingPolicy cachingPolicy = new NoCachingPolicy ();
            FileResult result = new FileResult (FileNameStrangeExtension, cachingPolicy);
            result.Apply (context);

            Assert.AreEqual ((int)HttpStatusCode.OK, context.StatusCode);
            Assert.IsNull(context.ResponseContentType);
        }

        [Test]
        public void WithCachingPolicyAndCacheVersionIsValid ()
        {
            const string ETag = "\"1\"";

            FileResult result = new FileResult (
                FileNameStrangeExtension,
                new CachingByETagPolicy (TimeSpan.FromDays (1), () => new Tuple<string, DateTime?>(ETag, timeService.CurrentTime)));
            result.StatusCode = (int)HttpStatusCode.Accepted;

            context.RequestHeaders[HttpConsts.HeaderIfNoneMatch] = ETag;

            result.Apply (context);
            Assert.AreEqual ((int)HttpStatusCode.NotModified, context.StatusCode);
        }

        [TestCase("gzip")]
        [TestCase("gzip,compress")]
        [TestCase(" gzip ,deflate")]
        [TestCase ("compress, gzip ,")]
        public void FileShouldBeCompressedIfRequested (string acceptEncodingValue)
        {
            context.RequestHeaders.Add(HttpConsts.HeaderAcceptEncoding, acceptEncodingValue);

            fileSystem.Stub (x => x.DoesFileExist (FileName)).Return (true);
            byte[] fileBytes = new byte[100];
            fileSystem.Stub (x => x.ReadFileAsBytes (FileName)).Return (fileBytes);

            ICachingPolicy cachingPolicy = new NoCachingPolicy ();
            FileResult result = new FileResult (FileName, cachingPolicy);
            result.AllowGzipCompression = true;
            result.Apply (context);

            Assert.AreEqual ((int)HttpStatusCode.OK, context.StatusCode);
            Assert.AreEqual ("gzip", context.ResponseHeaders[HttpConsts.HeaderContentEncoding]);
            Assert.AreEqual (HttpConsts.ContentTypeImagePng, context.ResponseContentType);
        }

        [Test]
        public void DoNotCompressIfNotAllowed()
        {
            context.RequestHeaders.Add(HttpConsts.HeaderAcceptEncoding, "gzip");

            fileSystem.Stub (x => x.DoesFileExist (FileName)).Return (true);
            byte[] fileBytes = new byte[100];
            fileSystem.Stub (x => x.ReadFileAsBytes (FileName)).Return (fileBytes);

            ICachingPolicy cachingPolicy = new NoCachingPolicy ();
            FileResult result = new FileResult (FileName, cachingPolicy);
            result.Apply (context);

            Assert.AreEqual ((int)HttpStatusCode.OK, context.StatusCode);
            Assert.AreEqual (null, context.ResponseHeaders[HttpConsts.HeaderTransferEncoding]);
            Assert.AreEqual (HttpConsts.ContentTypeImagePng, context.ResponseContentType);
        }

        [SetUp]
        public void Setup()
        {
            fileSystem = MockRepository.GenerateStub<IFileSystem>();
            timeService = new TimeMachine();
            IWebServerConfiguration configuration = MockRepository.GenerateStub<IWebServerConfiguration>();
            configuration.Stub(x => x.WebServerDevelopmentMode).Return(false);
            context = new FakeWebContext(null, null, fileSystem, timeService, configuration);
        }

        private FakeWebContext context;
        private IFileSystem fileSystem;
        private TimeMachine timeService;
        private const string FileName = "dir/file.png";
        private const string FileNameStrangeExtension = "dir/file.whatever";
    }
}