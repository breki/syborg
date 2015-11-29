using System.Net;

namespace Syborg
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public sealed class HttpListenerCookiesCollection : ICookiesCollection
    {
        public HttpListenerCookiesCollection(CookieCollection cookies)
        {
            this.cookies = cookies;
        }

        public void Add(ICookie cookie)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public ICookie Get(int index)
        {
            throw new System.NotImplementedException();
        }

        public ICookie Get(string name)
        {
            throw new System.NotImplementedException();
        }

        public string GetKey(int index)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(string name)
        {
            throw new System.NotImplementedException();
        }

        public void Set(ICookie cookie)
        {
            throw new System.NotImplementedException();
        }

        ICookie ICookiesCollection.this[int index]
        {
            get { throw new System.NotImplementedException(); }
        }

        ICookie ICookiesCollection.this[string name]
        {
            get { throw new System.NotImplementedException(); }
        }

        private readonly CookieCollection cookies;
    }
}