using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Reflection;
using LibroLib.FileSystem;
using LibroLib.Misc;
using log4net;
using Syborg.Razor;

namespace Syborg
{
    public class HttpListenerWebContext : IWebContext
    {
        public HttpListenerWebContext(
            HttpListenerContext context,
            string applicationUrl,
            string applicationPath, 
            IFileSystem fileSystem,
            IApplicationInfo applicationInfo,
            ITimeService timeService,
            IWebServerConfiguration configuration,
            IRazorViewRenderingEngine viewRenderingEngine,
            IFileMimeTypesMap fileMimeTypesMap)
        {
            Contract.Requires(context != null);
            Contract.Requires(context.Request != null);

            this.applicationPath = applicationPath;
            this.context = context;
            this.applicationUrl = applicationUrl;
            this.fileSystem = fileSystem;
            this.applicationInfo = applicationInfo;
            this.timeService = timeService;
            this.configuration = configuration;
            this.viewRenderingEngine = viewRenderingEngine;
            this.fileMimeTypesMap = fileMimeTypesMap;
            requestCookies = new HttpListenerCookiesCollection(context.Request.Cookies);
        }

        public IFileMimeTypesMap FileMimeTypesMap
        {
            get { return fileMimeTypesMap; }
        }

        public IFileSystem FileSystem
        {
            get { return fileSystem; }
        }

        public ITimeService TimeService
        {
            get { return timeService; }
        }

        public string ApplicationPath { get { return applicationPath; } }

        public string ApplicationUrl
        {
            get { return applicationUrl; }
        }

        public IWebServerConfiguration Configuration
        {
            get { return configuration; }
        }

        public Uri Url { get { return context.Request.Url; } }
        public Uri UrlReferrer { get { return context.Request.UrlReferrer; } }

        public IRazorViewRenderingEngine ViewRenderingEngine
        {
            get { return viewRenderingEngine; }
        }

        public string RequestContentType
        {
            get { return context.Request.ContentType; }
        }

        public ICookiesCollection RequestCookies
        {
            get { return requestCookies; }
        }

        public NameValueCollection RequestHeaders
        {
            get { return context.Request.Headers; }
        }

        public Stream RequestStream { get { return context.Request.InputStream; } }

        public long ResponseContentLength 
        {
            get { return context.Response.ContentLength64; }
            set { context.Response.ContentLength64 = value; } 
        }

        public string ResponseContentType
        {
            get { return context.Response.ContentType; }
            set { context.Response.ContentType = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public ICookiesCollection ResponseCookies
        {
            get { throw new NotImplementedException(); }
        }

        public string ResponseDescription { get; set; }

        public NameValueCollection ResponseHeaders
        {
            get { return context.Response.Headers; }
        }

        public bool ResponseSendChunked
        {
            get { return context.Response.SendChunked; }
            set { context.Response.SendChunked = value; }
        }

        public string HttpMethod
        {
            get { return context.Request.HttpMethod; }
        }

        public bool IsRequestLocal
        {
            get { return context.Request.IsLocal; }
        }

        public bool IsResponseClosed
        {
            get { return isResponseClosed; }
        }

        public bool IsSecureConnection
        {
            get { return context.Request.IsSecureConnection; }
        }

        public LoggingSeverity LoggingSeverity { get; set; }

        public IList<IWebPolicy> Policies
        {
            get { return policies; }
        }

        public Stream ResponseStream
        {
            get { return context.Response.OutputStream; }
        }

        public NameValueCollection QueryString { get { return context.Request.QueryString; } }

        public string RawUrl
        {
            get { return context.Request.RawUrl; }
        }

        public string UserHostAddress { get { return context.Request.UserHostAddress; } }
        public string UserHostName { get { return context.Request.UserHostName; } }

        public int StatusCode
        {
            get { return context.Response.StatusCode; }
            set { context.Response.StatusCode = value; }
        }

        public void AddHeader(string name, string value)
        {
            context.Response.AddHeader (name, value);
        }

        public void AddPolicy (IWebPolicy policy)
        {
            policies.Add (policy);
        }

        public void AppendCookie(ICookie cookie)
        {
            throw new NotImplementedException();
        }

        public void ApplyPolicies ()
        {
            foreach (IWebPolicy policy in policies)
                policy.Apply (this);
        }

        public void CloseResponse()
        {
            context.Response.Close();
            isResponseClosed = true;
        }

        public ICookie CreateCookie(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveHeader(string name)
        {
            context.Response.Headers.Remove(name);
        }

        public void SetCookie(ICookie cookie)
        {
            throw new NotImplementedException();
        }

        public string ToLogString()
        {
            throw new NotImplementedException();
        }

        private readonly string applicationPath;
        private readonly HttpListenerContext context;
        private readonly string applicationUrl;
        private readonly IFileSystem fileSystem;
        private readonly IApplicationInfo applicationInfo;
        private readonly HttpListenerCookiesCollection requestCookies;
        private readonly ITimeService timeService;
        private readonly IWebServerConfiguration configuration;
        private readonly IRazorViewRenderingEngine viewRenderingEngine;
        private readonly IFileMimeTypesMap fileMimeTypesMap;
        private readonly List<IWebPolicy> policies = new List<IWebPolicy>();
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool isResponseClosed;
    }
}