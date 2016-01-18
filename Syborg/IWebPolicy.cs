using System.Diagnostics.Contracts;

namespace Syborg
{
    [ContractClass (typeof(IWebPolicyContract))]
    public interface IWebPolicy
    {
        void Apply(IWebContext context);
    }

    [ContractClassFor(typeof(IWebPolicy))]
    // ReSharper disable once InconsistentNaming
    internal abstract class IWebPolicyContract : IWebPolicy
    {
        public void Apply(IWebContext context)
        {
            Contract.Requires(context != null);
            throw new System.NotImplementedException();
        }
    }
}