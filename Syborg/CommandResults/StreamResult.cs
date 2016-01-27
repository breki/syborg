using System.IO;
using System.Net;

namespace Syborg.CommandResults
{
    public class StreamResult : WebCommandResultBase
    {
        public StreamResult(Stream stream, string contentType)
        {
            this.stream = stream;
            AddHeader(HttpConsts.HeaderContentType, contentType);
        }

        public bool SendChunked
        {
            get { return sendChunked; }
            set { sendChunked = value; }
        }

        public override void Apply(IWebContext context)
        {
            ApplyEssentials (context);

            context.StatusCode = (int)HttpStatusCode.OK;
            context.ResponseSendChunked = SendChunked;
            stream.CopyTo(context.ResponseStream);
            context.ResponseDescription = "Returning stream data";
            context.ApplyPolicies ();
            context.CloseResponse ();
        }

        private readonly Stream stream;
        private bool sendChunked;
    }
}