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

            string text = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            byte[] uncompressedBytes = new UTF8Encoding (false).GetBytes (text);

            httpContext.Response.ClearHeaders();
            httpContext.Response.ContentType = "text/plain";

            //WriteUncompressed(httpContext, uncompressedBytes);
            GZipCompressAndWrite(httpContext, uncompressedBytes);
            //DeflateCompressAndWrite(httpContext, uncompressedBytes);

            //httpContext.Response.End();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private static void WriteUncompressed(HttpContext context, byte[] bytes)
        {
            string contentLenStr = bytes.Length.ToString (CultureInfo.InvariantCulture);
            HttpContext.Current.Response.Headers["Content-Length"] = contentLenStr;
            context.Response.Headers["Content-Length"] = contentLenStr;
            context.Response.BinaryWrite (bytes);
        }

        private static void GZipCompressAndWrite (HttpContext context, byte[] bytes)
        {
            byte[] compressed = GZipCompressByteArray (bytes);
            log.DebugFormat ("Compressed {0} bytes to {1}", bytes.Length, compressed.Length);

            string contentLenStr = compressed.Length.ToString (CultureInfo.InvariantCulture);
            //HttpContext.Current.Response.Headers["Content-Length"] = contentLenStr;
            //context.Response.Headers["Content-Length"] = contentLenStr;
            context.Response.AddHeader ("Content-Encoding", "gzip");

            //using (GZipStream compressStream = new GZipStream (context.Response.OutputStream, CompressionMode.Compress, true))
            //    compressStream.Write (bytes, 0, bytes.Length);

            context.Response.BinaryWrite (compressed);
        }

        //private static void DeflateCompressAndWrite (HttpContext context, byte[] bytes)
        //{
        //    context.Response.AddHeader (HttpConsts.HeaderContentEncoding, "deflate");
            
        //    byte[] compressed = DeflateCompressByteArray(bytes);
        //    log.DebugFormat("Compressed {0} bytes to {1}", bytes.Length, compressed.Length);

        //    HttpContext.Current.Response.Headers[HttpConsts.HeaderContentLength]
        //        = compressed.Length.ToString (CultureInfo.InvariantCulture);
        //    context.Response.BinaryWrite (compressed);
        //}

        private static byte[] GZipCompressByteArray (byte[] data)
        {
            using (MemoryStream outputStream = new MemoryStream ())
            {
                using (GZipStream compressStream = new GZipStream(outputStream, CompressionMode.Compress, false))
                    compressStream.Write (data, 0, data.Length);

                outputStream.Flush();
                return outputStream.ToArray ();
            }
        }

        //private static byte[] DeflateCompressByteArray (byte[] data)
        //{
        //    using (MemoryStream outputStream = new MemoryStream ())
        //    {
        //        using (DeflateStream compressStream = new DeflateStream (outputStream, CompressionMode.Compress, false))
        //        {
        //            compressStream.Write (data, 0, data.Length);
        //            compressStream.Close();
        //        }

        //        outputStream.Flush();
        //        return outputStream.ToArray ();
        //    }
        //}

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}