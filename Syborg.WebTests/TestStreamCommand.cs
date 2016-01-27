using System.IO;
using System.Text;
using Syborg.CommandResults;
using Syborg.Routing;

namespace Syborg.WebTests
{
    public class TestStreamCommand : IWebCommand
    {
        public IWebCommandResult Execute(IWebContext context, WebRequestRouteMatch routeMatch)
        {
            const string Text = "This is a stream response";
            MemoryStream stream = new MemoryStream(new UTF8Encoding(false).GetBytes(Text));

            return new StreamResult(stream, HttpConsts.ContentTypeTextPlain);
        }
    }
}