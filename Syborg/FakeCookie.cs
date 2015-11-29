using System;
using System.Collections.Specialized;

namespace Syborg
{
    public class FakeCookie : ICookie
    {
        public FakeCookie(string name)
        {
            Name = name;
        }

        public string Domain { get; set; }
        public DateTime Expires { get; set; }
        public bool HasKeys { get { return values.HasKeys(); } }
        public bool HttpOnly { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Secure { get; set; }
        public string Value { get; set; }
        public NameValueCollection Values { get { return values; } }

        public string this[string key]
        {
            get { return values[key]; }
            set { values[key] = value; }
        }

        private readonly NameValueCollection values = new NameValueCollection();
    }
}