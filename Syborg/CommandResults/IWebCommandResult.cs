using System.Diagnostics.Contracts;

namespace Syborg.CommandResults
{
    [ContractClass(typeof(IWebCommandResultContract))]
    public interface IWebCommandResult
    {
        void Apply(IWebContext context);
    }

    [ContractClassFor(typeof(IWebCommandResult))]
    // ReSharper disable once InconsistentNaming
    internal abstract class IWebCommandResultContract : IWebCommandResult
    {
        public void Apply(IWebContext context)
        {
            Contract.Requires(context != null);
        }
    }
}