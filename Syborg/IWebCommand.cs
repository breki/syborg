using System.Diagnostics.Contracts;
using Syborg.CommandResults;
using Syborg.Routing;

namespace Syborg
{
    [ContractClass(typeof(IWebCommandContract))]
    public interface IWebCommand
    {
        IWebCommandResult Execute(IWebContext context, WebRequestRouteMatch routeMatch);
    }

    [ContractClassFor (typeof(IWebCommand))]
    internal abstract class IWebCommandContract : IWebCommand
    {
        public IWebCommandResult Execute(IWebContext context, WebRequestRouteMatch routeMatch)
        {
            Contract.Requires(context != null);
            Contract.Requires(routeMatch != null);
            Contract.Ensures(Contract.Result<IWebCommandResult>() != null);
            throw new System.NotImplementedException();
        }
    }
}