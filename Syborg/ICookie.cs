using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Syborg
{
    [ContractClass(typeof(ICookieContract))]
    public interface ICookie
    {
        string Domain { get; set; }
        DateTime Expires { get; set; }
        bool HasKeys { get; }
        bool HttpOnly { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        bool Secure { get; set; }
        string Value { get; set; }
        NameValueCollection Values { get; }

        string this[string key] { get; set; }
    }

    [ContractClassFor(typeof(ICookie))]
    internal abstract class ICookieContract : ICookie
    {
        public string Domain
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public DateTime Expires
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool HasKeys
        {
            get { throw new NotImplementedException(); }
        }

        public bool HttpOnly
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new NotImplementedException();
            }

            set
            {
                Contract.Requires(value != null);
                throw new NotImplementedException();
            }
        }

        public string Path
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Secure
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Value
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public NameValueCollection Values
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string key]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}