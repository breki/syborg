using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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
            this.ApplicationUrl = applicationUrl;
            this.ApplicationPath = applicationPath;
            this.FileSystem = fileSystem;
            this.TimeService = timeService;
            this.Configuration = configuration;
        }

        public string ApplicationPath { get; }

        public string ApplicationUrl { get; }

        public IWebServerConfiguration Configuration { get; }

        public IFileMimeTypesMap FileMimeTypesMap { get; set; } 
            = new FileMimeTypesMap().RegisterStandardMimeTypes();

        public IFileSystem FileSystem { get; }

        public ITimeService TimeService { get; }

        public string HttpMethod { get; set; }
        public bool IsRequestLocal { get; set; }

        public bool IsResponseClosed { get; set; }

        public bool IsSecureConnection { get; set; }
        public LoggingSeverity LoggingSeverity { get; set; }

        public IList<IWebPolicy> Policies => policies;

        public Stream ResponseStream => responseStream;

        [SuppressMessage ("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public NameValueCollection QueryString { get; set; } = new NameValueCollection();

        public string RawUrl { get; set; }
        public string RequestContentType
        {
            get => RequestHeaders[HttpConsts.HeaderContentType];
            set => RequestHeaders[HttpConsts.HeaderContentType] = value;
        }

        public ICookiesCollection RequestCookies => requestCookies;

        [SuppressMessage(
            "Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public NameValueCollection RequestHeaders { get; set; } =
            new NameValueCollection();

        public Stream RequestStream { get; set; } = new MemoryStream();

        public long ResponseContentLength { get; set; }
        public string ResponseContentType
        {
            get => responseHeaders[HttpConsts.HeaderContentType];
            set => responseHeaders[HttpConsts.HeaderContentType] = value;
        }

        public ICookiesCollection ResponseCookies => responseCookies;

        public string ResponseDescription { get; set; }

        public NameValueCollection ResponseHeaders => responseHeaders;

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
            responseCookies.Add(cookie);
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
            responseCookies.Set(cookie);
       }

        public void EncodeFormDataToRequest (NameValueCollection formData)
        {
            Contract.Requires(formData != null);

            RequestContentType = HttpConsts.ContentTypeApplicationXWwwFormUrlencoded;

            string[] array = formData.AllKeys.SelectMany (
                formData.GetValues,
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
            RequestStream.Seek(0, SeekOrigin.Begin);
            return Encoding.UTF8.GetString(((MemoryStream)RequestStream).ToArray());
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

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                RequestStream?.Dispose();
                responseStream?.Dispose ();
            }

            disposed = true;
        }

        private readonly FakeCookiesCollection requestCookies = new FakeCookiesCollection ();
        private readonly FakeCookiesCollection responseCookies = new FakeCookiesCollection ();
        private readonly MemoryStream responseStream = new MemoryStream();
        private readonly WebHeaderCollection responseHeaders = new WebHeaderCollection();
        private readonly List<IWebPolicy> policies = new List<IWebPolicy>();
        private bool disposed;
    }
}