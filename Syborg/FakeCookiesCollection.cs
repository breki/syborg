using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Syborg
{
    [SuppressMessage(
        "Microsoft.Naming",
        "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public sealed class FakeCookiesCollection : ICookiesCollection
    {
        public void Add(ICookie cookie)
        {
            Set(cookie);
        }

        public void Clear()
        {
            cookiesDict.Clear();
            cookiesList.Clear();
        }

        public ICookie Get(int index)
        {
            return cookiesList[index];
        }

        public ICookie Get(string name)
        {
            cookiesDict.TryGetValue(name, out ICookie cookie);
            return cookie;
        }

        public string GetKey(int index)
        {
            return cookiesList[index].Name;
        }

        public void Remove(string name)
        {
            ICookie cookie = cookiesDict[name];
            cookiesDict.Remove(name);
            cookiesList.Remove(cookie);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "0")]
        public void Set(ICookie cookie)
        {
            cookiesDict[cookie.Name] = cookie;

            int index = cookiesList.FindIndex(x => x.Name == cookie.Name);
            if (index >= 0)
                cookiesList[index] = cookie;
            else
                cookiesList.Add(cookie);
        }

        ICookie ICookiesCollection.this[int index] => cookiesList[index];

        ICookie ICookiesCollection.this[string name] => Get(name);

        private readonly List<ICookie> cookiesList = new List<ICookie>();
        private readonly Dictionary<string, ICookie> cookiesDict =
            new Dictionary<string, ICookie>();
    }
}