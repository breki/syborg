using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using LibroLib.FileSystem;
using LibroLib.Misc;
using Syborg.Common;
using Syborg.Razor;

namespace Syborg
{
    [ContractClass(typeof(IWebContextContract))]
    public interface IWebContext
    {
        /// <summary>
        /// Gets the Web application's root path on the server.
        /// </summary>
        /// <example>For example, if the complete <see cref="Url"/> is 'http://localhost/azurite/map/maribor',
        /// then the <see cref="ApplicationPath"/> will be '/Azurite'.</example>
        [SuppressMessage ("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        string ApplicationPath { get; }

        /// <summary>
        /// Gets the full URL of the Web application's root path on the server.
        /// </summary>
        /// <example>For example, if the complete <see cref="Url"/> is 'http://localhost/azurite/map/maribor',
        /// then the <see cref="ApplicationUrl"/> will be 'http://localhost/Azurite'.</example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), SuppressMessage ("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        string ApplicationUrl { get; }

        IWebServerConfiguration Configuration { get; }
        IFileMimeTypesMap FileMimeTypesMap { get; }
        IFileSystem FileSystem { get; }
        string HttpMethod { get; }
        bool IsRequestLocal { get; }
        bool IsResponseClosed { get; }
        bool IsSecureConnection { get; }
        LoggingSeverity LoggingSeverity { get; set; }
        IList<IWebPolicy> Policies { get; }
        NameValueCollection QueryString { get; }

        /// <summary>
        /// Gets the raw (relative) URL of the request.
        /// </summary>
        /// <example>For example, if the complete <see cref="Url"/> is 'http://localhost/azurite/map/maribor',
        /// then the <see cref="RawUrl"/> will be '/azurite/map/maribor'.</example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), SuppressMessage ("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        string RawUrl { get; }

        string RequestContentType { get; }
        ICookiesCollection RequestCookies { get; }
        NameValueCollection RequestHeaders { get; }
        Stream RequestStream { get; }
        long ResponseContentLength { get; set; }
        string ResponseContentType { get; set; }
        ICookiesCollection ResponseCookies { get; }
        string ResponseDescription { get; set; }
        NameValueCollection ResponseHeaders { get; }
        bool ResponseSendChunked { get; set; }
        Stream ResponseStream { get; }
        int StatusCode { get; set; }
        ITimeService TimeService { get; }

        /// <summary>
        /// Gets the complete URL of the request.
        /// </summary>
        /// <seealso cref="ApplicationUrl"/>
        /// <seealso cref="RawUrl"/>
        Uri Url { get; }
        Uri UrlReferrer { get; }

        string UserHostAddress { get; }
        string UserHostName { get; }

        IRazorViewRenderingEngine ViewRenderingEngine { get; }

        /// <summary>
        /// Adds a HTTP header to the response.
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Header value</param>
        void AddHeader(string name, string value);
        void AddPolicy(IWebPolicy policy);
        void AppendCookie(ICookie cookie);
        void ApplyPolicies();
        void CloseResponse();
        ICookie CreateCookie(string name);
        void SetCookie(ICookie cookie);
        string ToLogString();
    }

    [ContractClassFor(typeof(IWebContext))]
    internal abstract class IWebContextContract : IWebContext
    {
        public string ApplicationPath
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ApplicationUrl
        {
            get
            {
                Contract.Ensures (Contract.Result<string>() != null);
                throw new NotImplementedException ();
            }
        }

        public IWebServerConfiguration Configuration
        {
            get
            {
                Contract.Ensures (Contract.Result<IWebServerConfiguration>() != null);
                throw new NotImplementedException ();
            }
        }

        public IFileMimeTypesMap FileMimeTypesMap
        {
            get
            {
                Contract.Ensures (Contract.Result<IFileMimeTypesMap>() != null);
                throw new NotImplementedException ();
            }
        }

        public IFileSystem FileSystem
        {
            get
            {
                Contract.Ensures (Contract.Result<IFileSystem>() != null);
                throw new NotImplementedException ();
            }
        }

        public string HttpMethod
        {
            get
            {
                Contract.Ensures (Contract.Result<string>() != null);
                throw new NotImplementedException ();
            }
        }

        public bool IsRequestLocal
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsResponseClosed
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSecureConnection
        {
            get { throw new NotImplementedException(); }
        }

        public LoggingSeverity LoggingSeverity
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IList<IWebPolicy> Policies
        {
            get
            {
                Contract.Ensures (Contract.Result<IList<IWebPolicy>>() != null);
                throw new NotImplementedException ();
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                Contract.Ensures (Contract.Result<NameValueCollection>() != null);
                throw new NotImplementedException ();
            }
        }

        public string RawUrl
        {
            get
            {
                Contract.Ensures (Contract.Result<string>() != null);
                throw new NotImplementedException ();
            }
        }

        public string RequestContentType
        {
            get { throw new NotImplementedException(); }
        }

        public ICookiesCollection RequestCookies
        {
            get
            {
                Contract.Ensures (Contract.Result<ICookiesCollection>() != null);
                throw new NotImplementedException ();
            }
        }

        public NameValueCollection RequestHeaders
        {
            get
            {
                Contract.Ensures (Contract.Result<NameValueCollection>() != null);
                throw new NotImplementedException();
            }
        }

        public Stream RequestStream
        {
            get
            {
                Contract.Ensures (Contract.Result<Stream>() != null);
                throw new NotImplementedException ();
            }
        }

        public long ResponseContentLength
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string ResponseContentType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ICookiesCollection ResponseCookies
        {
            get { throw new NotImplementedException(); }
        }

        public string ResponseDescription
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public NameValueCollection ResponseHeaders
        {
            get
            {
                Contract.Ensures (Contract.Result<NameValueCollection>() != null);
                throw new NotImplementedException ();
            }
        }

        public bool ResponseSendChunked
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Stream ResponseStream
        {
            get
            {
                Contract.Ensures (Contract.Result<Stream>() != null);
                throw new NotImplementedException ();
            }
        }

        public int StatusCode
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ITimeService TimeService
        {
            get
            {
                Contract.Ensures (Contract.Result<ITimeService>() != null);
                throw new NotImplementedException ();
            }
        }

        public Uri Url
        {
            get
            {
                Contract.Ensures (Contract.Result<Uri>() != null);
                throw new NotImplementedException ();
            }
        }

        public Uri UrlReferrer
        {
            get { throw new NotImplementedException(); }
        }

        public string UserHostAddress
        {
            get { throw new NotImplementedException(); }
        }

        public string UserHostName
        {
            get { throw new NotImplementedException(); }
        }

        public IRazorViewRenderingEngine ViewRenderingEngine
        {
            get
            {
                Contract.Ensures (Contract.Result<IRazorViewRenderingEngine>() != null);
                throw new NotImplementedException ();
            }
        }

        public void AddHeader(string name, string value)
        {
            Contract.Requires(name != null);
            throw new NotImplementedException();
        }

        public void AddPolicy(IWebPolicy policy)
        {
            Contract.Requires(policy != null);
            throw new NotImplementedException();
        }

        public void AppendCookie(ICookie cookie)
        {
            Contract.Requires(cookie != null);
            throw new NotImplementedException();
        }

        public void ApplyPolicies()
        {
            throw new NotImplementedException();
        }

        public void CloseResponse()
        {
            throw new NotImplementedException();
        }

        public ICookie CreateCookie(string name)
        {
            Contract.Requires(name != null);
            throw new NotImplementedException();
        }

        public void SetCookie(ICookie cookie)
        {
            Contract.Requires(cookie != null);
            throw new NotImplementedException();
        }

        public string ToLogString()
        {
            throw new NotImplementedException();
        }
    }
}