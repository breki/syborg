using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using LibroLib.FileSystem;
using LibroLib.Misc;
using Syborg.Razor;

namespace Syborg
{
    public class FakeWebContext : IWebContext, IDisposable
    {
        public FakeWebContext (
            string applicationUrl,
            string applicationPath, 
            IFileSystem fileSystem, 
            ITimeService timeService,
            IWebServerConfiguration configuration)
        {
            this.applicationUrl = applicationUrl;
            this.applicationPath = applicationPath;
            this.fileSystem = fileSystem;
            this.timeService = timeService;
            this.configuration = configuration;
        }

        public string ApplicationPath { get { return applicationPath; } }

        public string ApplicationUrl
        {
            get { return applicationUrl; }
        }

        public IWebServerConfiguration Configuration { get { return configuration; } }

        public IFileMimeTypesMap FileMimeTypesMap
        {
            get { return fileMimeTypesMap; }
            set { fileMimeTypesMap = value; }
        }

        public IFileSystem FileSystem { get { return fileSystem; } }

        public ITimeService TimeService
        {
            get { return timeService; }
        }

        public string HttpMethod { get; set; }
        public bool IsRequestLocal { get; set; }

        public bool IsResponseClosed { get; set; }

        public bool IsSecureConnection { get; set; }
        public LoggingSeverity LoggingSeverity { get; set; }

        public IList<IWebPolicy> Policies
        {
            get { return policies; }
        }

        public Stream ResponseStream
        {
            get { return responseStream; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public NameValueCollection QueryString
        {
            get { return queryString; }
            set { queryString = value; }
        }

        public string RawUrl { get; set; }
        public string RequestContentType
        {
            get { return requestHeaders[HttpConsts.HeaderContentType]; }
            set { requestHeaders[HttpConsts.HeaderContentType] = value; }
        }

        public ICookiesCollection RequestCookies
        {
            get { return requestCookies; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public NameValueCollection RequestHeaders
        {
            get { return requestHeaders; }
            set { requestHeaders = value; }
        }

        public Stream RequestStream
        {
            get { return requestStream; }
            set { requestStream = value; }
        }
        
        public long ResponseContentLength { get; set; }
        public string ResponseContentType
        {
            get { return responseHeaders[HttpConsts.HeaderContentType]; }
            set { responseHeaders[HttpConsts.HeaderContentType] = value; }
        }

        public ICookiesCollection ResponseCookies
        {
            get { return responseCookies; }
        }

        public string ResponseDescription { get; set; }

        public NameValueCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }

        public bool ResponseSendChunked { get; set; }

        public int StatusCode { get; set; }
        public Uri Url { get; set; }
        public Uri UrlReferrer { get; set; }
        public string UserHostAddress { get; set; }
        public string UserHostName { get; set; }

        public IRazorViewRenderingEngine ViewRenderingEngine { get; set; }

        public void AddHeader(string name, string value)
        {
            responseHeaders.Add(name, value);
        }

        public void AddPolicy (IWebPolicy policy)
        {
            policies.Add (policy);
        }

        public void AppendCookie(ICookie cookie)
        {
            throw new NotImplementedException ("todo");
        }

        public void ApplyPolicies ()
        {
            foreach (IWebPolicy policy in policies)
                policy.Apply (this);
        }

        public void CloseResponse()
        {
            IsResponseClosed = true;
        }

        public ICookie CreateCookie (string name)
        {
            return new FakeCookie(name);
        }

        public void RemoveHeader(string name)
        {
            responseHeaders.Remove(name);
        }

        public void SetCookie(ICookie cookie)
        {
            throw new NotImplementedException("todo");
        }

        public void EncodeFormDataToRequest (NameValueCollection formData)
        {
            Contract.Requires(formData != null);

            RequestContentType = HttpConsts.ContentTypeApplicationXWwwFormUrlencoded;

            string[] array = formData.AllKeys.SelectMany (
                key => formData.GetValues (key),
                (key, value) => string.Format (CultureInfo.InvariantCulture, "{0}={1}", HttpUtility.UrlEncode (key), HttpUtility.UrlEncode (value))).ToArray ();
            string encoded = string.Join ("&", array);

            byte[] bytes = Encoding.UTF8.GetBytes (encoded);
            RequestStream.Write(bytes, 0, bytes.Length);

            RequestStream.Seek(0, SeekOrigin.Begin);
        }

        public string ToLogString()
        {
            return null;
        }

        public string ReadRequestStreamAsText()
        {
            requestStream.Seek(0, SeekOrigin.Begin);
            return Encoding.UTF8.GetString(((MemoryStream)requestStream).ToArray());
        }

        public byte[] ReadResponseStreamAsByteArray()
        {
            responseStream.Seek (0, SeekOrigin.Begin);
            return responseStream.ToArray ();
        }

        public string ReadResponseStreamAsText()
        {
            responseStream.Seek (0, SeekOrigin.Begin);
            return Encoding.UTF8.GetString (responseStream.ToArray ());
        }

        public void SetRequestBody (string body)
        {
            Contract.Requires(body != null);

            RequestStream = new MemoryStream (Encoding.UTF8.GetBytes(body));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (requestStream != null)
                    requestStream.Dispose();
                if (responseStream != null)
                    responseStream.Dispose ();
            }

            disposed = true;
        }

        private readonly string applicationUrl;
        private readonly string applicationPath;
        private readonly IFileSystem fileSystem;
        private readonly ITimeService timeService;
        private readonly IWebServerConfiguration configuration;
        private readonly FakeCookiesCollection requestCookies = new FakeCookiesCollection ();
        private readonly FakeCookiesCollection responseCookies = new FakeCookiesCollection ();
        private Stream requestStream = new MemoryStream();
        private readonly MemoryStream responseStream = new MemoryStream();
        private NameValueCollection requestHeaders = new NameValueCollection();
        private readonly WebHeaderCollection responseHeaders = new WebHeaderCollection();
        private readonly List<IWebPolicy> policies = new List<IWebPolicy>();
        private NameValueCollection queryString = new NameValueCollection();
        private IFileMimeTypesMap fileMimeTypesMap = new FileMimeTypesMap().RegisterStandardMimeTypes();
        private bool disposed;
    }
}