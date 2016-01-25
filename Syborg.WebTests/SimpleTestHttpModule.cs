using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;

namespace Syborg.WebTests
{
    public class SimpleTestHttpModule : IHttpModule
    {
        public void Init (HttpApplication context)
        {
            context.BeginRequest += Application_BeginRequest;
        }

        public void Dispose()
        {
        }

        // ReSharper disable once InconsistentNaming
        private static void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext httpContext = application.Context;

            string text = @"The cave bear (Ursus spelaeus) was a species of bear that lived in Europe during the Pleistocene and became extinct about 24,000 years ago during the Last Glacial Maximum.
Both the name 'cave' and the scientific name spelaeus are because fossils of this species were mostly found in caves, showing that cave bears may have spent more time in caves than the brown bear, which uses caves only for hibernation. Consequently, in the course of time, whole layers of bones, almost entire skeletons, were found in many caves.";
            byte[] uncompressedBytes = new UTF8Encoding (false).GetBytes (text);

            httpContext.Response.ContentType = HttpConsts.ContentTypeTextPlain;

            GZipCompressAndWrite(httpContext, uncompressedBytes);
            //DeflateCompressAndWrite(httpContext, uncompressedBytes);

            httpContext.Response.End();
        }

        private static void GZipCompressAndWrite (HttpContext context, byte[] bytes)
        {
            context.Response.AddHeader (HttpConsts.HeaderContentEncoding, "gzip");

            byte[] compressed = GZipCompressByteArray (bytes);
            log.DebugFormat ("Compressed {0} bytes to {1}", bytes.Length, compressed.Length);

            HttpContext.Current.Response.Headers[HttpConsts.HeaderContentLength]
                = compressed.Length.ToString (CultureInfo.InvariantCulture);
            context.Response.BinaryWrite (compressed);
        }

        private static void DeflateCompressAndWrite (HttpContext context, byte[] bytes)
        {
            context.Response.AddHeader (HttpConsts.HeaderContentEncoding, "deflate");
            
            byte[] compressed = DeflateCompressByteArray(bytes);
            log.DebugFormat("Compressed {0} bytes to {1}", bytes.Length, compressed.Length);

            HttpContext.Current.Response.Headers[HttpConsts.HeaderContentLength]
                = compressed.Length.ToString (CultureInfo.InvariantCulture);
            context.Response.BinaryWrite (compressed);
        }

        private static byte[] GZipCompressByteArray (byte[] data)
        {
            using (MemoryStream outputStream = new MemoryStream ())
            {
                using (GZipStream compressStream = new GZipStream(outputStream, CompressionMode.Compress, false))
                {
                    compressStream.Write (data, 0, data.Length);
                    compressStream.Close();
                }

                outputStream.Flush();
                return outputStream.ToArray ();
            }
        }

        private static byte[] DeflateCompressByteArray (byte[] data)
        {
            using (MemoryStream outputStream = new MemoryStream ())
            {
                using (DeflateStream compressStream = new DeflateStream (outputStream, CompressionMode.Compress, false))
                {
                    compressStream.Write (data, 0, data.Length);
                    compressStream.Close();
                }

                outputStream.Flush();
                return outputStream.ToArray ();
            }
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}