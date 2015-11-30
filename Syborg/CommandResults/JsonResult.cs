using System.IO;
using System.Net;
using System.Text;
using LibroLib;
using Newtonsoft.Json;
using Syborg.Common;

namespace Syborg.CommandResults
{
    public class JsonResult : WebCommandResultBase
    {
        public JsonResult(object data)
        {
            this.data = data;
            StatusCode = (int)HttpStatusCode.OK;
        }

        public JsonResult (HttpStatusCode statusCode, object data)
        {
            this.data = data;
            StatusCode = (int)statusCode;
        }

        public object Data
        {
            get { return data; }
        }

        public bool IndentedFormatting
        {
            get { return indentedFormatting; }
            set { indentedFormatting = value; }
        }

        public override void Apply(IWebContext context)
        {
            ApplyEssentials(context);

            context.ResponseContentType = HttpConsts.ContentTypeApplicationJson;

            JsonSerializer serializer = new JsonSerializer();

            // use this if you want JavaScript-friendly date values
            //serializer.Converters.Add (new JavaScriptDateTimeConverter ());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = indentedFormatting ? Formatting.Indented : Formatting.None;

            byte[] bytes;
            using (MemoryStream tempOutStream = new MemoryStream())
            using (StreamWriter responseWriter = new StreamWriter(tempOutStream, new UTF8Encoding(false)))
            using (JsonWriter writer = new JsonTextWriter(responseWriter))
            {
                serializer.Serialize(writer, data);
                writer.Flush();
                context.ResponseContentLength = tempOutStream.Length;
                bytes = tempOutStream.ToArray();
            }

            context.ResponseStream.Write(bytes, 0, bytes.Length);

            context.ResponseDescription = "Returning JSON data (status: {0})".Fmt (StatusCode);

            //if (HttpLog.IsResponseLoggable ())
            //{
            //    string jsonBody;

            //    using (StringWriter stringWriter = new StringWriter ())
            //    using (JsonWriter logWriter = new JsonTextWriter (stringWriter))
            //    {
            //        serializer.Serialize (logWriter, data);
            //        jsonBody = stringWriter.ToString ();
            //    }

            //    Description = "Returning JSON data (status: {0}, body: {1})".Fmt(statusCode, jsonBody);
            //}

            context.ApplyPolicies ();
            context.CloseResponse();
        }

        private readonly object data;
        private bool indentedFormatting;
    }
}