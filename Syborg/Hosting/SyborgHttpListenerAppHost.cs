using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using LibroLib;
using LibroLib.FileSystem;
using LibroLib.Misc;
using log4net;
using Syborg.CommandResults;
using Syborg.Razor;
using Syborg.Routing;

namespace Syborg.Hosting
{
    public class SyborgHttpListenerAppHost : IDisposable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "webServer")]
        public SyborgHttpListenerAppHost (
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
        {
            Contract.Requires(viewRenderingEngine != null);
            Contract.Requires(routes != null);
            Contract.Requires(policies != null);

            this.configuration = configuration;
            this.externalUrl = externalUrl;
            this.serverPortNumber = serverPortNumber;
            this.applicationPath = applicationPath;
            this.fileSystem = fileSystem;
            this.applicationInfo = applicationInfo;
            this.timeService = timeService;
            this.viewRenderingEngine = viewRenderingEngine;
            this.fileMimeTypesMap = fileMimeTypesMap;
            this.webServerController = webServerController;

            this.routes.AddRange(routes);
            this.policies.AddRange(policies);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string ExternalUrl
        {
            get { return externalUrl; }
        }

        public int ServerPortNumber
        {
            get { return serverPortNumber; }
        }

        public string ApplicationPath
        {
            get { return applicationPath; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string FullWebServerUrl
        {
            get
            {
                StringBuilder url = new StringBuilder();
                url.Append(externalUrl);
                if (serverPortNumber != 80)
                    url.AppendFormat(":{0}", serverPortNumber);

                if (!string.IsNullOrEmpty(applicationPath))
                    url.Append(applicationPath);

                return url.ToString();
            }
        }

        public void Start ()
        {
            httpListener = new HttpListener ();
            
            string uriPrefix = "{0}:{1}{2}".Fmt(externalUrl, serverPortNumber, applicationPath);
            if (!uriPrefix.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                uriPrefix = uriPrefix + "/";
            log.DebugFormat (CultureInfo.InvariantCulture, "Reserving URL prefix '{0}'...", uriPrefix);

            httpListener.Prefixes.Add (uriPrefix);
            httpListener.Start ();

            log.InfoFormat (CultureInfo.InvariantCulture, "Serving on {0}".Fmt (FullWebServerUrl));
            log.Info("Press Ctrl+C to stop the server");

            IAsyncResult result = httpListener.BeginGetContext (WebRequestCallback, httpListener);
        }

        public void Stop()
        {
            webServerController.SignalToStop();
        }

        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        protected virtual void OnRequestReceived(IWebContext context)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "httpListener")]
        protected virtual void Dispose (bool disposing)
        {
            if (false == disposed)
            {
                // clean native resources         

                if (disposing)
                {
                    // clean managed resources
                    if (httpListener != null && httpListener.IsListening)
                        httpListener.Stop ();
                }

                disposed = true;
            }
        }

        private void WebRequestCallback (IAsyncResult result)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat ("WebRequestCallback (thread {0})", Thread.CurrentThread.ManagedThreadId);

            try
            {
                if (false == httpListener.IsListening)
                {
                    log.Info("Received web request, but HTTP listener is not listening");
                    return;
                }

                if (configuration.SimulatedResponseLag > TimeSpan.Zero)
                    Thread.Sleep(configuration.SimulatedResponseLag);

                HttpListenerContext listenerContext = httpListener.EndGetContext(result);
                Contract.Assume(listenerContext != null);

                IWebContext context = new HttpListenerWebContext(
                    listenerContext, FullWebServerUrl, applicationPath, fileSystem, applicationInfo, timeService, configuration, viewRenderingEngine, fileMimeTypesMap);

                OnRequestReceived(context);

                foreach (IWebPolicy policy in policies)
                    context.AddPolicy(policy);

                httpListener.BeginGetContext (WebRequestCallback, httpListener);

                if (log.IsDebugEnabled)
                    log.DebugFormat ("Request: {0} {1}", context.HttpMethod, context.RawUrl);
              
                // this is to prevent protocol violation error
                if (context.HttpMethod == "HEAD")
                {
                    context.CloseResponse ();
                    return;
                }

                IWebCommandResult webCommandResult = null;
                try
                {
                    foreach (IWebRequestRoute route in routes)
                    {
                        if (route.ProcessIfMatch (context, out webCommandResult))
                            break;
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    log.InfoFormat ("Error processing request: {0}", ex);
                    webCommandResult = new HttpStatusResult (HttpStatusCode.NotFound);
                }
                catch (Exception ex)
                {
                    webCommandResult = new HttpStatusResult (HttpStatusCode.InternalServerError);
                    log.Error ("InternalServerError - processing routes", ex);
                }

                if (webCommandResult == null)
                    webCommandResult = new HttpStatusResult (HttpStatusCode.BadRequest);

                try
                {
                    webCommandResult.Apply (context);
                }
                catch (Exception ex)
                {
                    webCommandResult = new HttpStatusResult (HttpStatusCode.InternalServerError);
                    log.Error ("InternalServerError - apply web command result", ex);
                    webCommandResult.Apply (context);
                }

                if (!context.IsResponseClosed)
                {
                    log.ErrorFormat("BUG: the response was not closed properly (thread {0}, URL={1})!", Thread.CurrentThread.ManagedThreadId, context.RawUrl);
                    context.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.CloseResponse();
                }
            }
            catch (HttpListenerException ex)
            {
                // An operation was attempted on a nonexistent network connection
                if ((uint)ex.ErrorCode == 0x80004005)
                {
                    //log.WarnFormat("HttpListenerException: {0:X}", ex.ErrorCode);
                    // ignore this
                }
                else
                    log.Warn ("HttpListenerException", ex);
            }
            catch (ObjectDisposedException ex)
            {
                log.Warn ("ObjectDisposedException", ex);
            }
            catch (Exception ex)
            {
                log.Fatal ("Web request processing thread failed", ex);
                webServerController.SignalToAbort ("Web request processing thread failed");
            }

            if (log.IsDebugEnabled)
                log.DebugFormat ("Finished WebRequestCallback (thread {0})", Thread.CurrentThread.ManagedThreadId);
        }

        private bool disposed;
        private HttpListener httpListener;
        private readonly List<IWebRequestRoute> routes = new List<IWebRequestRoute>();
        private readonly List<IWebPolicy> policies = new List<IWebPolicy>();
        private readonly IWebServerConfiguration configuration;
        private readonly string externalUrl;
        private readonly int serverPortNumber;
        private readonly string applicationPath;
        private readonly IFileSystem fileSystem;
        private readonly IApplicationInfo applicationInfo;
        private readonly ITimeService timeService;
        private readonly IRazorViewRenderingEngine viewRenderingEngine;
        private readonly IFileMimeTypesMap fileMimeTypesMap;
        private readonly IWebServerController webServerController;
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
    }
}