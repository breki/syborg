using System;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using log4net;
using Syborg.CommandResults;
using Syborg.Routing;

namespace Syborg.Hosting
{
    public class SyborgHttpModule : IHttpModule
    {
        public const string KeyAppHost = "AppHost";

        public void Init (HttpApplication context)
        {
            if (log.IsDebugEnabled)
                log.Debug("Init()");

            context.BeginRequest += Application_BeginRequest;
            context.EndRequest += Application_EndRequest;

            if (log.IsDebugEnabled)
                log.Debug("Init() finished");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        public void Dispose()
        {
            if (log.IsDebugEnabled)
                log.Debug("Dispose()");

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }

                disposed = true;
            }
        }

        // ReSharper disable once InconsistentNaming
        private static void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext httpContext = application.Context;

            try
            {
                SyborgHttpModuleAppHost appHost = (SyborgHttpModuleAppHost)application.Application[KeyAppHost];

                if (appHost.WebServerConfiguration.SimulatedResponseLag > TimeSpan.Zero)
                    Thread.Sleep (appHost.WebServerConfiguration.SimulatedResponseLag);
                
                IWebContext context = appHost.CreateWebContext(httpContext);

                //if (log.IsDebugEnabled)
                //    log.DebugFormat("Request: {0}", context.RawUrl);

                // this is to prevent protocol violation error
                if (context.HttpMethod == "HEAD")
                {
                    context.CloseResponse ();
                    return;
                }

                IWebCommandResult webCommandResult = null;
                try
                {
                    foreach (IWebRequestRoute route in appHost.Routes)
                    {
                        if (route.ProcessIfMatch (context, out webCommandResult))
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.FatalFormat ("Web request failed when routing: {0}", ex);
                    webCommandResult = new HttpStatusResult(
                        HttpStatusCode.InternalServerError,
                        "Ooops... an error occurred during the processing of your request. If this error persists, please contact our site administrator at info@somewhere.com. Thank you!");
                }

                if (webCommandResult == null)
                    webCommandResult = new HttpStatusResult (HttpStatusCode.BadRequest);

                webCommandResult.Apply (context);

                if (!context.IsResponseClosed)
                {
                    log.ErrorFormat (
                        CultureInfo.InvariantCulture,
                        "BUG: the response was not closed properly (thread {0}, URL={1})!", 
                        Thread.CurrentThread.ManagedThreadId, 
                        context.RawUrl);
                    context.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.CloseResponse ();
                }

                LogTraffic(context);
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
                log.FatalFormat ("Web request failed when applying the result: {0}", ex);
            }
        }

        // ReSharper disable once InconsistentNaming
        private static void Application_EndRequest(object sender, EventArgs e)
        {
            //HttpApplication application = (HttpApplication)sender;
            //HttpContext httpContext = application.Context;
            //NameValueCollection headers = httpContext.Response.Headers;
            //log.DebugFormat("resp. headers: {0}", headers.AllKeys.Concat (x => "{0}={1}".Fmt(x, headers[x]), "|"));
        }

        private static void LogTraffic(IWebContext context)
        {
            bool shouldLogTraffic;
            switch (context.LoggingSeverity)
            {
                case LoggingSeverity.Verbose:
                    shouldLogTraffic = httpLog.IsDebugEnabled;
                    break;
                case LoggingSeverity.Normal:
                    shouldLogTraffic = httpLog.IsInfoEnabled;
                    break;
                case LoggingSeverity.Error:
                    shouldLogTraffic = httpLog.IsErrorEnabled;
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (!shouldLogTraffic) 
                return;

            string logString = context.ToLogString();

            switch (context.LoggingSeverity)
            {
                case LoggingSeverity.Verbose:
                    httpLog.Debug(logString);
                    break;
                case LoggingSeverity.Normal:
                    httpLog.Info(logString);
                    break;
                case LoggingSeverity.Error:
                    httpLog.Error(logString);
                    log.Warn(logString);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private bool disposed;
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
        private static readonly ILog httpLog = LogManager.GetLogger ("HttpLog");
    }
}