using System;
using System.IO;
using LibroLib.FileSystem;
using LibroLib.Misc;
using Syborg.Commands;
using Syborg.ContentHandling;
using Syborg.Hosting;
using Syborg.Policies;
using Syborg.Razor;
using Syborg.Routing;

namespace Syborg.WebTests
{
    public class SyborgTestHttpModuleAppHost : SyborgHttpModuleAppHost
    {
        public SyborgTestHttpModuleAppHost()
        {
            IFileSystem fileSystem = new WindowsFileSystem ();
            IApplicationInfo applicationInfo = new ApplicationInfo ();
            ITimeService timeService = new RealTimeService ();
            IRazorCompiler razorCompiler = new InMemoryRazorCompiler ();
            IRazorViewRenderingEngine viewRenderingEngine = new RazorViewRenderingEngine (fileSystem, razorCompiler);

            IWebServerConfiguration config = new WebServerConfiguration();

            FileMimeTypesMap fileMimeTypesMap = new FileMimeTypesMap ().RegisterStandardMimeTypes ();

            Initialize (config, fileSystem, applicationInfo, timeService, fileMimeTypesMap, viewRenderingEngine);

            IFileCache fileCache = new FileCache ();

            string webAppRootDir;

            if (!WebServerConfiguration.WebServerDevelopmentMode)
            {
#if NCRUNCH
                                webAppRootDir = @"D:\hg\ScalableMaps\WebApp\ScalableMaps\ScalableMaps.Web2";
#else
                webAppRootDir = ApplicationInfo.GetAppDirectoryPath ("..");
#endif
            }
            else
                webAppRootDir = ApplicationInfo.GetAppDirectoryPath ("..");

            ContentCommand contentCommand = RegisterWebContent (webAppRootDir, fileCache, config);
            TestStreamCommand streamCommand = new TestStreamCommand();

            AddRoute (new RegexWebRequestRoute ("^Content/(?<path>.+)$", HttpMethod.GET, contentCommand));            
            AddRoute (new RegexWebRequestRoute ("^stream/(?<path>.+)$", HttpMethod.GET, streamCommand));

            AddPolicies(new IWebPolicy[] { new SecureResponseHeadersPolicy() });
        }

        private ContentCommand RegisterWebContent (string webAppRootDir, IFileCache fileCache, IWebServerConfiguration config)
        {
            string contentRootDirectory = Path.GetFullPath (Path.Combine (webAppRootDir, "Content"));

            ContentCommand contentCommand = new ContentCommand (contentRootDirectory, FileSystem, fileCache);
            contentCommand.CacheByMaxAge (@"cached-by-max-age.txt", TimeSpan.FromDays (30));
            contentCommand.CacheByETag (@"cached-by-etag.txt", TimeSpan.FromDays(10), () => GenerateContentETag (config));
            return contentCommand;
        }

        private static Tuple<string, DateTime?> GenerateContentETag (IWebServerConfiguration config)
        {
            return new Tuple<string, DateTime?>("\"1234\"", DateTime.Now);
        }
    }
}