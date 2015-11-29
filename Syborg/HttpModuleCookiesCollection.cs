using System.Web;

namespace Syborg
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public sealed class HttpModuleCookiesCollection : ICookiesCollection
    {
        public HttpModuleCookiesCollection(HttpCookieCollection cookies)
        {
            this.cookies = cookies;
        }

        public void Add(ICookie cookie)
        {
            cookies.Add(((HttpModuleCookie)cookie).NativeCookie);
        }

        public void Clear()
        {
            cookies.Clear();
        }

        public ICookie Get(int index)
        {
            return new HttpModuleCookie(cookies.Get(index));
        }

        public ICookie Get(string name)
        {
            HttpCookie nativeCookie = cookies.Get(name);
            if (nativeCookie == null)
                return null;

            return new HttpModuleCookie(nativeCookie);
        }

        public string GetKey(int index)
        {
            return cookies.GetKey(index);
        }

        public void Remove(string name)
        {
            cookies.Remove(name);
        }

        public void Set(ICookie cookie)
        {
            cookies.Set(((HttpModuleCookie)cookie).NativeCookie);
        }

        ICookie ICookiesCollection.this[int index]
        {
            get { return new HttpModuleCookie(cookies[index]); }
        }

        ICookie ICookiesCollection.this[string name]
        {
            get
            {
                HttpCookie nativeCookie = cookies[name];
                if (nativeCookie == null)
                    return null;

                return new HttpModuleCookie (nativeCookie);
            }
        }

        private readonly HttpCookieCollection cookies;
    }
}