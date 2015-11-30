using System.Net;
using System.Text;
using LibroLib;
using Syborg.Common;

namespace Syborg.CommandResults
{
    public class HttpStatusResult : WebCommandResultBase
    {
        public HttpStatusResult(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
        }

        public HttpStatusResult(HttpStatusCode statusCode, string message)
        {
            this.message = message;
            StatusCode = (int)statusCode;
        }

        public override void Apply (IWebContext context)
        {
            ApplyEssentials(context);

            if (message != null)
            {
                byte[] data = new UTF8Encoding(false).GetBytes(message);
                context.ResponseContentLength = data.Length;
                context.ResponseStream.Write(data, 0, data.Length);
                context.CloseResponse();
                context.ResponseDescription = "Response status code: {0}, message: {1}".Fmt (StatusCode, message);
            }
            else
                context.ResponseDescription = "Response status code: {0}".Fmt (StatusCode);

            context.ApplyPolicies ();
            context.CloseResponse();
        }

        public static HttpStatusResult Ok
        {
            get { return new HttpStatusResult(HttpStatusCode.OK); }
        }

        public static HttpStatusResult BadRequest
        {
            get
            {
                HttpStatusResult httpStatusResult = new HttpStatusResult(HttpStatusCode.BadRequest);
                httpStatusResult.LoggingSeverity = Syborg.LoggingSeverity.Verbose;
                return httpStatusResult;
            }
        }

        public static HttpStatusResult NotFound
        {
            get
            {
                HttpStatusResult httpStatusResult = new HttpStatusResult(HttpStatusCode.NotFound);
                httpStatusResult.LoggingSeverity = Syborg.LoggingSeverity.Verbose;
                return httpStatusResult;
            }
        }

        private readonly string message;
    }
}