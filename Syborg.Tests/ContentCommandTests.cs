using System;
using System.IO;
using System.Linq;
using System.Net;
using LibroLib.FileSystem;
using LibroLib.Misc;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.Caching;
using Syborg.CommandResults;
using Syborg.Commands;
using Syborg.ContentHandling;
using Syborg.Routing;

namespace Syborg.Tests
{
    public class ContentCommandTests
    {
        [Test]
        public void FetchFile()
        {
            routeMatch.AddParameter ("path", @"somedir/somepath.png");
            const string ExpectedFileName = @"d:\somedir\contents\somedir\somepath.png";
            fileSystem.Stub(x => x.DoesFileExist(ExpectedFileName)).Return(true);

            IWebCommandResult result = cmd.Execute(context, routeMatch);
            FileResult fileResult = (FileResult)result;
            Assert.AreEqual(ExpectedFileName, fileResult.FileName);
        }

        [Test]
        public void FileDoesNotExist()
        {
            routeMatch.AddParameter("path", @"somedir/somepath.png");
            IWebCommandResult result = cmd.Execute(context, routeMatch);
            HttpStatusResult fileResult = (HttpStatusResult)result;
            Assert.AreEqual((int)HttpStatusCode.NotFound, fileResult.StatusCode);
        }

        [Test]
        public void EnsureAbsolutePathsAreDisabled()
        {
            routeMatch.AddParameter ("path", @"d:/somedir/somepath.png");
            IWebCommandResult result = cmd.Execute (context, routeMatch);
            HttpStatusResult fileResult = (HttpStatusResult)result;
            Assert.AreEqual ((int)HttpStatusCode.BadRequest, fileResult.StatusCode);
        }

        [Test]
        public void EnsurePathsOutsideContentDirAreForbidden()
        {
            routeMatch.AddParameter ("path", @"..\forbidden\web.config");
            IWebCommandResult result = cmd.Execute (context, routeMatch);
            HttpStatusResult fileResult = (HttpStatusResult)result;
            Assert.AreEqual ((int)HttpStatusCode.Forbidden, fileResult.StatusCode);
        }

        [Test]
        public void NoCachingIsDoneByDefault ()
        {
            routeMatch.AddParameter ("path", @"somedir/somepath.png");
            const string ExpectedFileName = @"d:\somedir\contents\somedir\somepath.png";
            fileSystem.Stub (x => x.DoesFileExist (ExpectedFileName)).Return (true);

            IWebCommandResult result = cmd.Execute (context, routeMatch);
            FileResult fileResult = (FileResult)result;
            Assert.IsInstanceOf<NoCachingPolicy>(fileResult.CachingPolicy);
        }

        [Test]
        public void SettingCachingRuleShouldWork ()
        {
            TimeSpan maxAge = TimeSpan.FromDays(7);
            cmd.CacheByMaxAge (@"somedir\*", maxAge);

            routeMatch.AddParameter ("path", @"somedir\somepath.png");
            const string ExpectedFileName = @"d:\somedir\contents\somedir\somepath.png";
            fileSystem.Stub (x => x.DoesFileExist (ExpectedFileName)).Return (true);
            
            IFileInformation fileInfo = MockRepository.GenerateStub<IFileInformation>();
            fileInfo.LastWriteTime = now.AddDays(-3);
            fileSystem.Stub(x => x.GetFileInformation(ExpectedFileName)).Return(fileInfo);

            fileSystem.Stub(x => x.ReadFileAsBytes(ExpectedFileName)).Return(new byte[100]);

            IWebCommandResult result = cmd.Execute (context, routeMatch);
            FileResult fileResult = (FileResult)result;
            CachingByMaxAgePolicy maxAgePolicy = (CachingByMaxAgePolicy)fileResult.CachingPolicy;
            Assert.AreEqual(maxAge, maxAgePolicy.MaxAge);

            context.RequestHeaders.Add(HttpConsts.HeaderIfModifiedSince, fileInfo.LastWriteTime.ToRfc2822DateTime());
            result.Apply(context);

            Assert.AreEqual((int)HttpStatusCode.NotModified, context.StatusCode);
        }

        [Test]
        public void SettingCachingRuleDoesNotMatch ()
        {
            TimeSpan maxAge = TimeSpan.FromDays(7);
            cmd.CacheByMaxAge (@"somedir2\*", maxAge);

            routeMatch.AddParameter ("path", @"somedir\somepath.png");
            const string ExpectedFileName = @"d:\somedir\contents\somedir\somepath.png";
            fileSystem.Stub (x => x.DoesFileExist (ExpectedFileName)).Return (true);

            IWebCommandResult result = cmd.Execute (context, routeMatch);
            FileResult fileResult = (FileResult)result;
            Assert.IsInstanceOf<NoCachingPolicy>(fileResult.CachingPolicy);
        }

        [Test]
        public void CheckThatFileExists()
        {
            const string SampleFilePath = "image/somewhere.png";

            fileSystem.Stub(x => x.DoesFileExist(Path.GetFullPath(Path.Combine(ContentRootDirectory, SampleFilePath))))
                .Return(true);
            Assert.IsTrue(cmd.DoesFileExist(SampleFilePath));
        }

        [Test]
        public void TestCachingByETag()
        {
            TimeSpan maxAge = TimeSpan.FromDays (30);
            cmd.CacheByETag(
                @"somedir\*", 
                maxAge, 
                () => new Tuple<string, DateTime?>("xyz", new DateTime(2016, 01, 19, 10, 22, 0)));

            routeMatch.AddParameter ("path", @"somedir\somepath.png");
            const string ExpectedFileName = @"d:\somedir\contents\somedir\somepath.png";
            fileSystem.Stub (x => x.DoesFileExist (ExpectedFileName)).Return (true);

            IFileInformation fileInfo = MockRepository.GenerateStub<IFileInformation>();
            fileInfo.LastWriteTime = now.AddDays(-3);
            fileSystem.Stub (x => x.GetFileInformation (ExpectedFileName)).Return (fileInfo);

            fileSystem.Stub (x => x.ReadFileAsBytes (ExpectedFileName)).Return (new byte[100]);

            IWebCommandResult result = cmd.Execute (context, routeMatch);
            FileResult fileResult = (FileResult)result;
            CachingByETagPolicy cachingPolicy = (CachingByETagPolicy)fileResult.CachingPolicy;
            Assert.AreEqual (maxAge, cachingPolicy.MaxAge);

            context.RequestHeaders.Add (HttpConsts.HeaderIfNoneMatch, "xyz");
            result.Apply (context);

            Assert.AreEqual ((int)HttpStatusCode.NotModified, context.StatusCode);
        }

        [SetUp]
        public void Setup()
        {
            fileSystem = MockRepository.GenerateStub<IFileSystem>();
            fileCache = null;
            cmd = new ContentCommand(ContentRootDirectory, fileSystem, fileCache);

            IWebServerConfiguration webServerConfiguration = MockRepository.GenerateStub<IWebServerConfiguration>();

            now = new DateTime(2014, 06, 18, 7, 11, 0);
            timeService = MockRepository.GenerateStub<ITimeService>();
            
            context = new FakeWebContext(null, null, fileSystem, timeService, webServerConfiguration);

            routeMatch = new WebRequestRouteMatch ();
        }

        private ContentCommand cmd;
        private FakeWebContext context;
        private WebRequestRouteMatch routeMatch;
        private IFileSystem fileSystem;
        private IFileCache fileCache;
        private ITimeService timeService;
        private DateTime now;
        private const string ContentRootDirectory = @"d:\somedir\contents";
    }
}