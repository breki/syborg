using System.Diagnostics.Contracts;

namespace Syborg
{
    [ContractClass(typeof(ICookiesCollectionContract))]
    public interface ICookiesCollection
    {
        void Add(ICookie cookie);
        void Clear();
        ICookie Get(int index);
        ICookie Get(string name);
        string GetKey(int index);
        void Remove(string name);
        void Set(ICookie cookie);
        
        ICookie this[int index] { get; }
        ICookie this[string name] { get; }
    }

    [ContractClassFor(typeof(ICookiesCollection))]
    internal abstract class ICookiesCollectionContract : ICookiesCollection
    {
        public void Add(ICookie cookie)
        {
            Contract.Requires(cookie != null);
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public ICookie Get(int index)
        {
            Contract.Ensures(Contract.Result<ICookie>() != null);
            throw new System.NotImplementedException();
        }

        public ICookie Get(string name)
        {
            Contract.Requires(name != null);
            throw new System.NotImplementedException();
        }

        public string GetKey(int index)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            throw new System.NotImplementedException();
        }

        public void Remove(string name)
        {
            Contract.Requires(name != null);
            throw new System.NotImplementedException();
        }

        public void Set(ICookie cookie)
        {
            Contract.Requires(cookie != null);
            throw new System.NotImplementedException();
        }

        ICookie ICookiesCollection.this[int index]
        {
            get
            {
                Contract.Ensures(Contract.Result<ICookie>() != null);
                throw new System.NotImplementedException();
            }
        }

        ICookie ICookiesCollection.this[string name]
        {
            get
            {
                Contract.Requires(name != null);
                throw new System.NotImplementedException();
            }
        }
    }
}