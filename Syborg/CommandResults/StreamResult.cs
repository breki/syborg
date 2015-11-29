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

        public override void Apply(IWebContext context)
        {
            ApplyEssentials (context);

            context.StatusCode = (int)HttpStatusCode.OK;
            context.ResponseSendChunked = true;
            CopyStream(stream, context.ResponseStream);
            context.ResponseDescription = "Returning stream data";
            context.ApplyPolicies ();
            context.CloseResponse ();
        }

        private static long CopyStream (Stream input, Stream output)
        {
            byte[] buffer = new byte[512 * 1024];

            long streamLength = 0;
            while (true)
            {
                int len = input.Read (buffer, 0, buffer.Length);
                
                if (len <= 0) 
                    break;
                
                output.Write (buffer, 0, len);

                streamLength += len;
            }

            return streamLength;
        }

        private readonly Stream stream;
    }
}