using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace Syborg
{
    [SuppressMessage ("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    [ContractClass (typeof(ICookiesCollectionContract))]
    public interface ICookiesCollection
    {
        void Add([NotNull] ICookie cookie);
        void Clear();
        [SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        [NotNull]
        ICookie Get(int index);
        [SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        [CanBeNull]
        ICookie Get([NotNull] string name);
        [NotNull]
        string GetKey(int index);
        void Remove([NotNull] string name);
        [SuppressMessage ("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
        void Set([NotNull] ICookie cookie);
        
        [NotNull]
        ICookie this[int index] { get; }
        [CanBeNull]
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