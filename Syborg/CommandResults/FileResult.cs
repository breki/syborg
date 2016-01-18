using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using LibroLib;
using log4net;
using Syborg.Caching;

namespace Syborg.CommandResults
{
    public class FileResult : WebCommandResultBase
    {
        public FileResult(string fileName, ICachingPolicy cachingPolicy)
        {
            Contract.Requires (fileName != null);
            Contract.Requires (cachingPolicy != null);

            this.fileName = fileName;
            this.cachingPolicy = cachingPolicy;
        }

        public string FileName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return fileName;
            }
        }

        public ICachingPolicy CachingPolicy
        {
            get
            {
                Contract.Ensures(Contract.Result<ICachingPolicy>() != null);
                return cachingPolicy;
            }
        }

        public override void Apply (IWebContext context)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Apply (fileName='{0}')", fileName);

            cachingPolicy.ProcessRequest(fileName, context, ReturnFile);

            if (log.IsDebugEnabled)
                log.DebugFormat ("Finished Apply (fileName='{0}')", fileName);
        }

        private static void SetContentTypeForRequestedFile (IWebContext context, string fileFullPath)
        {
            string contentType = context.FileMimeTypesMap.GetContentType(fileFullPath);

            if (contentType == null)
                log.Warn("Unknown content type for file '{0}'".Fmt(fileFullPath));
            else
                context.ResponseContentType = contentType;
        }

        private void ReturnFile(object fileNameObj, IWebContext context)
        {
            Contract.Requires(fileNameObj != null);
            Contract.Requires(context != null);

            string fileNameUsed = (string)fileNameObj;

            if (log.IsDebugEnabled)
                log.DebugFormat("ReturnFile (fileName='{0}')", fileNameUsed);

            // handle case when the file is missing
            if (!context.FileSystem.DoesFileExist(fileNameUsed))
            {
                context.StatusCode = (int)HttpStatusCode.NotFound;
                context.ResponseDescription = "File '{0}' does not exist".Fmt (fileNameUsed);
            }
            else
            {
                byte[] fileData = context.FileSystem.ReadFileAsBytes(fileNameUsed);

                SetContentTypeForRequestedFile(context, fileNameUsed);

                fileData = CompressFileIfRequested(context, fileData);

                context.StatusCode = (int)HttpStatusCode.OK;
                context.ResponseContentLength = fileData.Length;

                using (BinaryWriter responseWriter = new BinaryWriter(context.ResponseStream))
                {
                    responseWriter.Write(fileData);
                    context.ResponseDescription = "Returning file '{0}'".Fmt(fileNameUsed);
                }
            }

            ApplyEssentials (context);

            context.ApplyPolicies ();
            context.CloseResponse ();

            if (log.IsDebugEnabled)
                log.DebugFormat ("ReturnFile (fileName='{0}') finished", fileNameUsed);
        }

        private static byte[] CompressFileIfRequested(IWebContext context, byte[] fileData)
        {
            bool compressFile = false;
            string acceptEncodingHeaderValue = context.RequestHeaders[HttpConsts.HeaderAcceptEncoding];
            if (acceptEncodingHeaderValue != null)
            {
                string[] acceptedEncodings = acceptEncodingHeaderValue.Split(',');
                if (acceptedEncodings.Any(x => x.Trim() == "gzip"))
                    compressFile = true;
            }

            if (compressFile)
            {
                fileData = CompressByteArray(fileData);
                context.ResponseHeaders.Add(HttpConsts.HeaderTransferEncoding, "gzip");
            }

            return fileData;
        }

        private static byte[] CompressByteArray(byte[] data)
        {
            using (MemoryStream outputStream = new MemoryStream(data))
            using (GZipStream compressStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                compressStream.Write (data, 0, data.Length);
                compressStream.Flush();
                return outputStream.ToArray();
            }
        }

        private readonly string fileName;
        private readonly ICachingPolicy cachingPolicy;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}