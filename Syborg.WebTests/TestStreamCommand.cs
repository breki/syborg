using System.IO;
using System.Net;
using System.Text;
using LibroLib;
using Syborg.CommandResults;
using Syborg.Routing;

namespace Syborg.WebTests
{
    public class TestStreamCommand : IWebCommand
    {
        public IWebCommandResult Execute(IWebContext context, WebRequestRouteMatch routeMatch)
        {
            string path = routeMatch["path"];

            if (path == null)
                return new HttpStatusResult (HttpStatusCode.BadRequest);

            switch (path)
            {
                case "short-text":
                    return ReturnShortText();
                case "long-binary":
                    return ReturnLongBinary();
                default:
                    return new HttpStatusResult(HttpStatusCode.BadRequest);
            }
        }

        private static IWebCommandResult ReturnShortText()
        {
            const string Text = "This is a stream response";
            MemoryStream stream = new MemoryStream (new UTF8Encoding (false).GetBytes (Text));

            return new StreamResult (stream, HttpConsts.ContentTypeTextPlain);
        }

        private static IWebCommandResult ReturnLongBinary()
        {
            const int StreamLength = 4 * 1024 * 1024;
            byte[] data = new byte[StreamLength];
            for (int i = 0; i < StreamLength; i++)
                data[i] = (byte)(i % 256);

            MemoryStream stream = new MemoryStream (data);
            return new StreamResult (stream, HttpConsts.ContentTypeApplicationPdf);
        }
    }
}