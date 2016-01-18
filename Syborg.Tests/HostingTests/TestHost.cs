using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LibroLib.FileSystem;
using LibroLib.Misc;
using Syborg.Hosting;
using Syborg.Razor;
using Syborg.Routing;

namespace Syborg.Tests.HostingTests
{
    [ExcludeFromCodeCoverage]
    public class TestHost : SyborgHttpListenerAppHost
    {
        public TestHost(
            IWebServerConfiguration configuration, 
            string externalUrl, 
            int serverPortNumber, 
            string applicationPath, 
            IFileSystem fileSystem, 
            IApplicationInfo applicationInfo, 
            ITimeService timeService, 
            IRazorViewRenderingEngine viewRenderingEngine, 
            IFileMimeTypesMap fileMimeTypesMap, 
            IWebServerController webServerController, 
            IEnumerable<IWebRequestRoute> routes, 
            IEnumerable<IWebPolicy> policies)
            : base(configuration, externalUrl, serverPortNumber, applicationPath, fileSystem, applicationInfo, timeService, viewRenderingEngine, fileMimeTypesMap, webServerController, routes, policies)
        {
        }
    }
}