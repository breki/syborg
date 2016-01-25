using System.Diagnostics.Contracts;

namespace Syborg
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    [ContractClass (typeof(ICookiesCollectionContract))]
    public interface ICookiesCollection
    {
        void Add(ICookie cookie);
        void Clear();
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        ICookie Get(int index);
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        ICookie Get(string name);
        string GetKey(int index);
        void Remove(string name);
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
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