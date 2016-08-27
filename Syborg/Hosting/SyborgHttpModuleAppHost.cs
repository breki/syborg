using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web;
using LibroLib.FileSystem;
using LibroLib.Misc;
using Syborg.Razor;
using Syborg.Routing;

namespace Syborg.Hosting
{
    public class SyborgHttpModuleAppHost : IDisposable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
        public IWebServerConfiguration WebServerConfiguration
        {
            get { return webServerConfiguration; }
        }

        public IApplicationInfo ApplicationInfo
        {
            get { return applicationInfo; }
        }

        public IFileSystem FileSystem
        {
            get { return fileSystem; }
        }

        public ITimeService TimeService
        {
            get { return timeService; }
        }

        public IFileMimeTypesMap FileMimeTypesMap
        {
            get { return fileMimeTypesMap; }
        }

        public IRazorViewRenderingEngine ViewRenderingEngine
        {
            get { return viewRenderingEngine; }
        }

        public IList<IWebRequestRoute> Routes
        {
            get { return routes; }
        }

        public SyborgHttpModuleAppHost AddRoute (IWebRequestRoute route)
        {
            Contract.Requires(route != null);
            routes.Add (route);
            return this;
        }

        public void AddRoutes (IEnumerable<IWebRequestRoute> routes)
        {
            Contract.Requires(routes != null);
            foreach (IWebRequestRoute route in routes)
                AddRoute(route);
        }

        public void AddPolicies (IEnumerable<IWebPolicy> policies)
        {
            Contract.Requires(policies != null);
            this.policies.AddRange(policies);
        }

        public IWebContext CreateWebContext(HttpContext httpContext)
        {
            Contract.Requires(httpContext != null);
            Contract.Ensures(Contract.Result<IWebContext>() != null);

            HttpModuleWebContext context = new HttpModuleWebContext(httpContext, fileSystem, timeService, webServerConfiguration, viewRenderingEngine, fileMimeTypesMap);
            foreach (IWebPolicy policy in policies)
                context.AddPolicy (policy);
            return context;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "webServer")]
        protected void Initialize(
            IWebServerConfiguration webServerConfiguration,
            IFileSystem fileSystem,
            IApplicationInfo applicationInfo,
            ITimeService timeService,
            IFileMimeTypesMap fileMimeTypesMap,
            IRazorViewRenderingEngine viewRenderingEngine)
        {
            this.webServerConfiguration = webServerConfiguration;
            this.fileSystem = fileSystem;
            this.applicationInfo = applicationInfo;
            this.timeService = timeService;
            this.fileMimeTypesMap = fileMimeTypesMap;
            this.viewRenderingEngine = viewRenderingEngine;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                // clean native resources         

                if (disposing)
                {
                    // clean managed resources        
                    DisposeOfManagedResources();
                }

                disposed = true;
            }
        }

        protected virtual void DisposeOfManagedResources()
        {
        }

        private IWebServerConfiguration webServerConfiguration;
        private IFileSystem fileSystem;
        private IApplicationInfo applicationInfo;
        private ITimeService timeService;
        private IRazorViewRenderingEngine viewRenderingEngine;
        private bool disposed;
        private readonly List<IWebRequestRoute> routes = new List<IWebRequestRoute>();
        private readonly List<IWebPolicy> policies = new List<IWebPolicy>();
        private IFileMimeTypesMap fileMimeTypesMap;
    }
}