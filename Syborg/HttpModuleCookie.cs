using System;
using System.Collections.Specialized;
using System.Web;

namespace Syborg
{
    public class HttpModuleCookie : ICookie
    {
        public HttpModuleCookie(HttpCookie cookie)
        {
            this.cookie = cookie;
        }

        public HttpCookie NativeCookie
        {
            get { return cookie; }
        }

        public string Domain
        {
            get { return cookie.Domain; }
            set { cookie.Domain = value; }
        }

        public DateTime Expires
        {
            get { return cookie.Expires; }
            set { cookie.Expires = value; }
        }

        public bool HasKeys
        {
            get { return cookie.HasKeys; }
        }

        public bool HttpOnly
        {
            get { return cookie.HttpOnly; }
            set { cookie.HttpOnly = value; }
        }

        public string Name
        {
            get { return cookie.Name; }
            set { cookie.Name = value; }
        }

        public string Path
        {
            get { return cookie.Path; }
            set { cookie.Path = value; }
        }

        public bool Secure
        {
            get { return cookie.Secure; }
            set { cookie.Secure = value; }
        }

        public string Value
        {
            get { return cookie.Value; }
            set { cookie.Value = value; }
        }

        public NameValueCollection Values
        {
            get { return cookie.Values; }
        }

        public string this[string key]
        {
            get { return cookie[key]; }
            set { cookie[key] = value; }
        }

        private readonly HttpCookie cookie;
    }
}