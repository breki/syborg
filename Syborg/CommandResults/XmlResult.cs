using System.Net;
using System.Text;
using LibroLib;
using Syborg.Common;

namespace Syborg.CommandResults
{
    public class XmlResult : WebCommandResultBase
    {
        public XmlResult (string xml)
        {
            this.xml = xml;
            StatusCode = (int?)HttpStatusCode.OK;
        }

        public XmlResult (string xml, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            this.xml = xml;
            StatusCode = (int)statusCode;
        }

        public string Xml
        {
            get { return xml; }
        }

        public override void Apply (IWebContext context)
        {
            ApplyEssentials (context);

            context.ResponseContentType = HttpConsts.ContentTypeTextXmlUtf8;

            byte[] data = new UTF8Encoding (false).GetBytes (xml);
            context.ResponseContentLength = data.Length;
            context.ResponseStream.Write (data, 0, data.Length);
            context.ResponseDescription = "Returning XML data (len: {0})".Fmt (xml.Length);
            context.ApplyPolicies ();
            context.CloseResponse ();
        }

        private readonly string xml;
    }
}