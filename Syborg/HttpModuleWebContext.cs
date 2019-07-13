using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using LibroLib;
using LibroLib.FileSystem;
using LibroLib.Misc;
using log4net;
using Syborg.Razor;

namespace Syborg
{
    public class HttpModuleWebContext : IWebContext
    {
        public HttpModuleWebContext(
            HttpContext context,
            IFileSystem fileSystem,
            ITimeService timeService,
            IWebServerConfiguration configuration,
            IRazorViewRenderingEngine viewRenderingEngine,
            IFileMimeTypesMap fileMimeTypesMap)
        {
            Contract.Requires(context != null);

            this.context = context;
            this.FileSystem = fileSystem;
            this.TimeService = timeService;
            this.Configuration = configuration;
            this.ViewRenderingEngine = viewRenderingEngine;
            this.FileMimeTypesMap = fileMimeTypesMap;
            requestCookies = new HttpModuleCookiesCollection(context.Request.Cookies);
            responseCookies = new HttpModuleCookiesCollection(context.Response.Cookies);
        }

        public string ApplicationPath => context.Request.ApplicationPath;

        public string ApplicationUrl => context.Request.Url.GetLeftPart (UriPartial.Authority) + context.Request.ApplicationPath;

        public IWebServerConfiguration Configuration { get; }

        public IFileMimeTypesMap FileMimeTypesMap { get; }

        public IFileSystem FileSystem { get; }

        public ITimeService TimeService { get; }

        public string HttpMethod => context.Request.HttpMethod;

        public bool IsRequestLocal => context.Request.IsLocal;

        public bool IsResponseClosed { get; private set; }

        public bool IsSecureConnection => context.Request.IsSecureConnection;

        public LoggingSeverity LoggingSeverity { get; set; }

        public IList<IWebPolicy> Policies => policies;

        public Stream ResponseStream => context.Response.OutputStream;

        public NameValueCollection QueryString => context.Request.QueryString;

        public string RawUrl => context.Request.RawUrl;

        public string RequestContentType => context.Request.ContentType;

        public ICookiesCollection RequestCookies => requestCookies;

        public NameValueCollection RequestHeaders => context.Request.Headers;

        public Stream RequestStream => context.Request.InputStream;

        public long ResponseContentLength
        {
            get
            {
                string lenString = context.Response.Headers[HttpConsts.HeaderContentLength];
                if (lenString == null)
                    return -1;

                long contentLength;
                if (long.TryParse(lenString, NumberStyles.Integer, CultureInfo.InvariantCulture, out contentLength))
                    return contentLength;

                return -1;
            }

            set
            {
                string lenToString = value.ToString(CultureInfo.InvariantCulture);

                //context.Response.Headers[HttpConsts.HeaderContentLength] = lenToString;

                // NOTE: this is a workaround for setting the Content-Length, since setting it in context.Response
                // seems to be ignored by ASP.NET
                HttpContext.Current.Response.Headers[HttpConsts.HeaderContentLength] = lenToString;
            }
        }
        
        public string ResponseContentType
        {
            get => context.Response.ContentType;
            set => context.Response.ContentType = value;
        }

        public ICookiesCollection ResponseCookies => responseCookies;

        public string ResponseDescription { get; set; }

        public NameValueCollection ResponseHeaders => context.Response.Headers;

        // currently doesn't have any functionality
        // see http://serialseb.com/blog/2008/10/05/httpresponseflush-forces-chunked/
        public bool ResponseSendChunked
        {
            get => throw new InvalidOperationException("currently not implemented, since HttpResponse doesn't have the SendChunked property");

            set
            {
            }
        }

        public int StatusCode
        {
            get => context.Response.StatusCode;
            set => context.Response.StatusCode = value;
        }

        public Uri Url => context.Request.Url;

        public Uri UrlReferrer => context.Request.UrlReferrer;

        public string UserHostAddress => context.Request.UserHostAddress;

        public string UserHostName => context.Request.UserHostName;

        public IRazorViewRenderingEngine ViewRenderingEngine { get; }

        public void AddHeader(string name, string value)
        {
            context.Response.AddHeader(name, value);
        }

        public void AddPolicy(IWebPolicy policy)
        {
            policies.Add(policy);
        }

        public void AppendCookie(ICookie cookie)
        {
            context.Response.AppendCookie(
                ((HttpModuleCookie)cookie).NativeCookie);
        }

        public void ApplyPolicies()
        {
            foreach (IWebPolicy policy in policies)
                policy.Apply(this);
        }

        public void CloseResponse()
        {
            if (IsResponseClosed)
                throw new InvalidOperationException("The response has already been closed");

            context.ApplicationInstance.CompleteRequest();
            IsResponseClosed = true;
        }

        public ICookie CreateCookie(string name)
        {
            return new HttpModuleCookie(new HttpCookie(name));
        }

        public void RemoveHeader(string name)
        {
            context.Response.Headers.Remove(name);
        }

        public void SetCookie(ICookie cookie)
        {
            context.Response.SetCookie(((HttpModuleCookie)cookie).NativeCookie);
        }

        public string ToLogString()
        {
            try
            {
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;

                StringBuilder s = new StringBuilder ();
                s.AppendFormat (CultureInfo.InvariantCulture, "IP={0}, ", request.UserHostAddress);
                s.AppendFormat (CultureInfo.InvariantCulture, "{0} ", response.StatusCode);
                s.AppendFormat (CultureInfo.InvariantCulture, "{0} {1}, ", request.HttpMethod, request.Url);
                if (request.UrlReferrer != null)
                    s.AppendFormat (CultureInfo.InvariantCulture, "(ref {0}), ", request.UrlReferrer);

                s.Append ("req. headers: (");
                AppendHeadersToLog (s, request.Headers);
                s.Append ("), ");

                s.AppendFormat (CultureInfo.InvariantCulture, "ua: '{0}' | ", request.UserAgent);

                s.AppendFormat (CultureInfo.InvariantCulture, "status={0} ", response.StatusCode);
                if (!string.IsNullOrEmpty (response.StatusDescription))
                    s.AppendFormat (CultureInfo.InvariantCulture, "({0}), ", response.StatusDescription);
                else
                    s.Append (", ");

                s.AppendFormat (CultureInfo.InvariantCulture, "ctype={0}, ", response.ContentType);
                s.AppendFormat (CultureInfo.InvariantCulture, "len={0}, ", ResponseContentLength);

                s.Append ("resp. headers: (");
                AppendHeadersToLog(s, response.Headers);
                s.Append (") ");

                return s.ToString();
            }
            catch (Exception ex)
            {
                log.Error ("Error logging request", ex);
                return "[error]";
            }
        }

        private static void AppendHeadersToLog(StringBuilder s, NameValueCollection headers)
        {
            s.Append (headers.AllKeys.Concat (x => "{0}={1}".Fmt(x, headers[x]), "|"));
        }

        private readonly HttpContext context;
        private readonly HttpModuleCookiesCollection requestCookies;
        private readonly HttpModuleCookiesCollection responseCookies;
        private readonly List<IWebPolicy> policies = new List<IWebPolicy>();
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
    }
}