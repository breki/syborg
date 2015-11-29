using System.Diagnostics.Contracts;
using System.Net;
using System.Text;
using Syborg.Caching;
using Syborg.Common;

namespace Syborg.CommandResults
{
    public class TextResult : WebCommandResultBase
    {
        public TextResult()
        {
            StatusCode = (int)HttpStatusCode.OK;
        }

        public TextResult(string text)
        {
            Contract.Requires(text != null);

            this.text = text;
            StatusCode = (int)HttpStatusCode.OK;
        }

        public TextResult(string text, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Contract.Requires(text != null);

            this.text = text;
            StatusCode = (int)statusCode;
        }

        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                Contract.Requires(value != null);
                text = value;
            }
        }

        public ICachingPolicy CachingPolicy
        {
            get { return cachingPolicy; }
            set { cachingPolicy = value; }
        }

        public string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }

        public override void Apply (IWebContext context)
        {
            if (cachingPolicy != null)
                cachingPolicy.ProcessRequest(null, context, ReturnData);
            else
                ReturnData(null, context);
        }

        private void ReturnData (object resourceData, IWebContext context)
        {
            context.ResponseContentType = contentType;

            byte[] data = new UTF8Encoding (false).GetBytes (text);
            context.ResponseContentLength = data.Length;
            context.ResponseStream.Write (data, 0, data.Length);
            context.ResponseDescription = "Returning text data (len: {0})".Fmt (text.Length);
            ApplyEssentials (context);
            context.ApplyPolicies ();
            context.CloseResponse ();
        }

        private string text;
        private string contentType = HttpConsts.ContentTypeTextPlainUtf8;
        private ICachingPolicy cachingPolicy;
    }
}