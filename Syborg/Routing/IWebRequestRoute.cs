using System.Diagnostics.Contracts;
using Syborg.CommandResults;

namespace Syborg.Routing
{
    [ContractClass(typeof(IWebRequestRouteContract))]
    public interface IWebRequestRoute
    {
        bool ProcessIfMatch (IWebContext context, out IWebCommandResult result);
    }

    [ContractClassFor(typeof(IWebRequestRoute))]
    internal abstract class IWebRequestRouteContract : IWebRequestRoute
    {
        public bool ProcessIfMatch(IWebContext context, out IWebCommandResult result)
        {
            Contract.Requires(context != null);
            throw new System.NotImplementedException();
        }
    }
}