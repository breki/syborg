using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using LibroLib;
using log4net;
using Syborg.Caching;
using Syborg.ContentHandling;

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

        public bool AllowGzipCompression
        {
            get { return allowGzipCompression; }
            set { allowGzipCompression = value; }
        }

        public IFileCache FileCache
        {
            get { return fileCache; }
            set { fileCache = value; }
        }

        public override void Apply (IWebContext context)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Apply (fileName='{0}')", fileName);

            cachingPolicy.ProcessRequest(fileName, context, HandleFileRequest);

            if (log.IsDebugEnabled)
                log.DebugFormat ("Finished Apply (fileName='{0}')", fileName);
        }

        private static void SetContentTypeForRequestedFile (IWebContext context, string fileFullPath)
        {
            Contract.Requires (context != null);
            Contract.Requires (fileFullPath != null);

            string contentType = context.FileMimeTypesMap.GetContentType (fileFullPath);

            if (contentType == null)
                log.Warn("Unknown content type for file '{0}'".Fmt(fileFullPath));
            else
                context.ResponseContentType = contentType;
        }

        private void HandleFileRequest(object fileNameObj, IWebContext context)
        {
            Contract.Requires(fileNameObj != null);
            Contract.Requires(context != null);

            string fileNameUsed = (string)fileNameObj;

            if (log.IsDebugEnabled)
                log.DebugFormat("HandleFileRequest (fileName='{0}')", fileNameUsed);

            FetchFile(context, fileNameUsed);

            ApplyEssentials (context);

            context.ApplyPolicies ();
            context.CloseResponse ();

            if (log.IsDebugEnabled)
                log.DebugFormat ("HandleFileRequest (fileName='{0}') finished", fileNameUsed);
        }

        private void FetchFile(IWebContext context, string fileNameUsed)
        {
            Contract.Requires(context != null);
            Contract.Requires(fileNameUsed != null);

            string transferEncoding;
            bool shouldFileBeCompressed = DetermineIfFileShouldBeCompressed(context, out transferEncoding);
            if (fileCache != null)
            {
                CachableFileInfo fileInfo;
                if (fileCache.TryGetFile(fileNameUsed, transferEncoding, out fileInfo))
                {
                    RespondWithCachedFile(context, fileInfo);
                    return;
                }
            }

            // handle case when the file is missing
            bool doesFileExist = context.FileSystem.DoesFileExist(fileNameUsed);

            if (!doesFileExist)
                RespondFileNotExist(context, fileNameUsed);
            else
                RespondWithFile(context, fileNameUsed, shouldFileBeCompressed);
        }

        private bool DetermineIfFileShouldBeCompressed(
            IWebContext context, out string transferEncoding)
        {
            Contract.Requires(context != null);

            transferEncoding = null;

            if (!allowGzipCompression)
                return false;
            
            string acceptEncodingHeaderValue = context.RequestHeaders[HttpConsts.HeaderAcceptEncoding];
            if (acceptEncodingHeaderValue == null)
                return false;

            string[] acceptedEncodings = acceptEncodingHeaderValue.Split(',');
            if (acceptedEncodings.Any(x => x.Trim() == "gzip"))
            {
                transferEncoding = "gzip";
                return true;
            }

            return false;
        }

        private static void RespondFileNotExist(IWebContext context, string fileNameUsed)
        {
            Contract.Requires (context != null);

            context.StatusCode = (int)HttpStatusCode.NotFound;
            context.ResponseDescription = "File '{0}' does not exist".Fmt(fileNameUsed);
        }

        private void RespondWithFile(
            IWebContext context, string fileNameUsed, bool shouldFileBeCompressed)
        {
            Contract.Requires (context != null);

            byte[] fileData = context.FileSystem.ReadFileAsBytes (fileNameUsed);

            SetContentTypeForRequestedFile(context, fileNameUsed);

            if (shouldFileBeCompressed)
                fileData = CompressFileData(context, fileData);

            context.StatusCode = (int)HttpStatusCode.OK;
            context.ResponseContentLength = fileData.Length;

            using (BinaryWriter responseWriter = new BinaryWriter(context.ResponseStream))
            {
                responseWriter.Write(fileData);
                context.ResponseDescription = "Returning file '{0}'".Fmt(fileNameUsed);
            }

            if (fileCache != null)
            {
                fileCache.CacheFile(
                    fileNameUsed, 
                    fileData, 
                    context.ResponseHeaders[HttpConsts.HeaderContentEncoding]);
            }
        }

        private static void RespondWithCachedFile (IWebContext context, CachableFileInfo fileInfo)
        {
            Contract.Requires (context != null);
            Contract.Requires (fileInfo != null);

            byte[] fileData = fileInfo.FileData;

            SetContentTypeForRequestedFile (context, fileInfo.FileName);

            context.StatusCode = (int)HttpStatusCode.OK;
            context.ResponseContentLength = fileData.Length;

            if (fileInfo.TransferEncoding != null)
                context.AddHeader (HttpConsts.HeaderContentEncoding, fileInfo.TransferEncoding);

            using (BinaryWriter responseWriter = new BinaryWriter (context.ResponseStream))
            {
                responseWriter.Write (fileData);
                context.ResponseDescription = "Returning file '{0}'".Fmt (fileInfo.FileName);
            }
        }

        private static byte[] CompressFileData(IWebContext context, byte[] fileData)
        {
            Contract.Requires (context != null);
            Contract.Requires(fileData != null);
            Contract.Ensures(Contract.Result<byte[]>() != null);

            fileData = CompressByteArray (fileData);
            context.AddHeader (HttpConsts.HeaderContentEncoding, "gzip");
            return fileData;
        }

        private static byte[] CompressByteArray(byte[] data)
        {
            Contract.Requires (data != null);
            Contract.Ensures(Contract.Result<byte[]>() != null);

            using (MemoryStream outputStream = new MemoryStream ())
            {
                using (GZipStream compressStream = new GZipStream (outputStream, CompressionMode.Compress))
                    compressStream.Write (data, 0, data.Length);

                return outputStream.ToArray ();
            }
        }

        private readonly string fileName;
        private readonly ICachingPolicy cachingPolicy;
        private bool allowGzipCompression;
        private IFileCache fileCache;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}