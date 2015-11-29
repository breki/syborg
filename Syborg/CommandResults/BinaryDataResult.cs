using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using Syborg.Caching;
using Syborg.Common;

namespace Syborg.CommandResults
{
    public class BinaryDataResult : WebCommandResultBase
    {
        public BinaryDataResult (byte[] data, ICachingPolicy cachingPolicy)
        {
            Contract.Requires(data != null);
            Contract.Requires(cachingPolicy != null);

            this.data = data;
            this.cachingPolicy = cachingPolicy;
        }

        public override void Apply (IWebContext context)
        {
            cachingPolicy.ProcessRequest (null, context, ReturnData);
        }

        private void ReturnData(object resourceData, IWebContext context)
        {
            context.StatusCode = (int)HttpStatusCode.OK;
            context.ResponseContentLength = data.Length;

            using (BinaryWriter responseWriter = new BinaryWriter (context.ResponseStream))
            {
                responseWriter.Write (data);
                context.ResponseDescription = "Returning binary data (len: {0})".Fmt (data.Length);
            }

            ApplyEssentials (context);

            context.ApplyPolicies ();
            context.CloseResponse ();
        }

        private readonly byte[] data;
        private readonly ICachingPolicy cachingPolicy;
    }
}