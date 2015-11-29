using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;
using Syborg.Common;
using Syborg.Razor;

namespace Syborg
{
    public class HttpModuleWebContext : IWebContext
    {
        public HttpModuleWebContext(
            HttpContext context,
            IFileSystem fileSystem,
            IApplicationInfo applicationInfo,
            ITimeService timeService,
            IWebServerConfiguration configuration,
            IRazorViewRenderingEngine viewRenderingEngine,
            IFileMimeTypesMap fileMimeTypesMap)
        {
            Contract.Requires(context != null);

            this.context = context;
            this.fileSystem = fileSystem;
            this.applicationInfo = applicationInfo;
            this.timeService = timeService;
            this.configuration = configuration;
            this.viewRenderingEngine = viewRenderingEngine;
            this.fileMimeTypesMap = fileMimeTypesMap;
            requestCookies = new HttpModuleCookiesCollection(context.Request.Cookies);
            responseCookies = new HttpModuleCookiesCollection(context.Response.Cookies);
        }

        public string ApplicationPath
        {
            get { return context.Request.ApplicationPath; }
        }

        public string ApplicationUrl
        {
            get { return context.Request.Url.GetLeftPart (UriPartial.Authority) + context.Request.ApplicationPath; }
        }

        public IWebServerConfiguration Configuration
        {
            get { return configuration; }
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

        public string HttpMethod { get { return context.Request.HttpMethod; } }
        public bool IsRequestLocal
        {
            get { return context.Request.IsLocal; }
        }

        public bool IsResponseClosed
        {
            get { return isResponseClosed; }
        }

        public bool IsSecureConnection { get { return context.Request.IsSecureConnection; } }
        public LoggingSeverity LoggingSeverity { get; set; }

        public IList<IWebPolicy> Policies
        {
            get { return policies; }
        }

        public Stream ResponseStream { get { return context.Response.OutputStream; } }
        public NameValueCollection QueryString { get { return context.Request.QueryString; } }
        public string RawUrl { get { return context.Request.RawUrl; } }
        public string RequestContentType { get { return context.Request.ContentType; } }

        public ICookiesCollection RequestCookies
        {
            get { return requestCookies; }
        }

        public NameValueCollection RequestHeaders { get { return context.Request.Headers; } }
        public Stream RequestStream { get { return context.Request.InputStream; } }
        
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
                context.Response.Headers[HttpConsts.HeaderContentLength] = value.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        public string ResponseContentType
        {
            get { return context.Response.ContentType; }
            set { context.Response.ContentType = value; }
        }

        public ICookiesCollection ResponseCookies
        {
            get { return responseCookies; }
        }

        public string ResponseDescription { get; set; }

        public NameValueCollection ResponseHeaders { get { return context.Response.Headers; } }
        
        // currently not implemented, since HttpResponse doesn't have the SendChunked property
        public bool ResponseSendChunked { get; set; }

        public int StatusCode
        {
            get { return context.Response.StatusCode; }
            set { context.Response.StatusCode = value; }
        }

        public Uri Url { get { return context.Request.Url; } }
        public Uri UrlReferrer { get { return context.Request.UrlReferrer; } }

        public string UserHostAddress { get { return context.Request.UserHostAddress; } }
        public string UserHostName { get { return context.Request.UserHostName; } }

        public IRazorViewRenderingEngine ViewRenderingEngine { get { return viewRenderingEngine; } }

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
            context.Response.AppendCookie(((HttpModuleCookie)cookie).NativeCookie);
        }

        public void ApplyPolicies()
        {
            foreach (IWebPolicy policy in policies)
                policy.Apply(this);
        }

        public void CloseResponse()
        {
            context.ApplicationInstance.CompleteRequest();
            isResponseClosed = true;
        }

        public ICookie CreateCookie(string name)
        {
            return new HttpModuleCookie(new HttpCookie(name));
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

                s.Append ("headers: (");
                foreach (string header in request.Headers.AllKeys)
                {
                    string value = request.Headers[header];
                    s.AppendFormat (CultureInfo.InvariantCulture, "{0}={1},", header, value);
                }

                s.Append ("), ");

                s.AppendFormat (CultureInfo.InvariantCulture, "ua: '{0}' | ", request.UserAgent);

                s.AppendFormat (CultureInfo.InvariantCulture, "status={0} ", response.StatusCode);
                if (!string.IsNullOrEmpty (response.StatusDescription))
                    s.AppendFormat (CultureInfo.InvariantCulture, "({0}), ", response.StatusDescription);
                else
                    s.Append (", ");

                s.AppendFormat (CultureInfo.InvariantCulture, "ctype={0}, ", response.ContentType);
                s.AppendFormat (CultureInfo.InvariantCulture, "len={0}, ", ResponseContentLength);

                s.Append ("headers: (");
                foreach (string header in response.Headers.AllKeys)
                {
                    string value = request.Headers[header];
                    s.AppendFormat (CultureInfo.InvariantCulture, "{0}={1},", header, value);
                }

                s.Append (") ");

                return s.ToString();
            }
            catch (Exception ex)
            {
                log.Error ("Error logging request", ex);
                return "[error]";
            }
        }

        private readonly HttpContext context;
        private readonly IFileSystem fileSystem;
        private readonly IApplicationInfo applicationInfo;
        private readonly HttpModuleCookiesCollection requestCookies;
        private readonly HttpModuleCookiesCollection responseCookies;
        private readonly ITimeService timeService;
        private readonly IWebServerConfiguration configuration;
        private readonly IRazorViewRenderingEngine viewRenderingEngine;
        private readonly IFileMimeTypesMap fileMimeTypesMap;
        private readonly List<IWebPolicy> policies = new List<IWebPolicy>();
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
        private bool isResponseClosed;
    }
}