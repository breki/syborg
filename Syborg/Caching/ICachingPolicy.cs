using System;
using System.Diagnostics.Contracts;

namespace Syborg.Caching
{
    /// <summary>
    /// Interface for implementing various types of web request caching policies.
    /// Caching policy determines when to return a fresh version of a resource.
    /// </summary>
    [ContractClass (typeof(ICachingPolicyContract))]
    public interface ICachingPolicy
    {
        /// <summary>
        /// Processes the web request according to the caching policy. 
        /// </summary>
        /// <remarks>Depending on the policy itself, if the resource is not yet cached or the cache is stale,
        /// the method should call <see cref="returnResourceAction"/> method to return the new resource value. 
        /// Otherwise, the method should return appropriate HTTP headers in the web response.</remarks>
        /// <param name="resourceData">Resource data to return in case the resource is not yet cached or the cache is stale
        /// (in which case the resourceData value will be used to call <see cref="returnResourceAction"/> method.
        /// Can be <c>null</c> if <see cref="returnResourceAction"/> method is designed to calculate the new resource value
        /// itself.</param>
        /// <param name="context">Web context</param>
        /// <param name="returnResourceAction">The method to be called in case the resource is not yet cached 
        /// or the cache is stale. Typically this method fills the web response object with all the resource data needed.</param>
        void ProcessRequest (object resourceData, IWebContext context, Action<object, IWebContext> returnResourceAction);
    }

    [ContractClassFor(typeof(ICachingPolicy))]
    internal abstract class ICachingPolicyContract : ICachingPolicy
    {
        public void ProcessRequest(object resourceData, IWebContext context, Action<object, IWebContext> returnResourceAction)
        {
            Contract.Requires(context != null);
            Contract.Requires(returnResourceAction != null);
        }
    }
}