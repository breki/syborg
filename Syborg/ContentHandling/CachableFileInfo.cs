using System.Diagnostics.Contracts;

namespace Syborg.ContentHandling
{
    /// <summary>
    /// Holds information about the content file that is cached for HTTP serving.
    /// </summary>
    public class CachableFileInfo
    {
        public CachableFileInfo(
            string fileName,
            byte[] fileData,
            string transferEncoding = null)
        {
            Contract.Requires (fileName != null);
            Contract.Requires (fileData != null);

            this.fileName = fileName;
            this.fileData = fileData;
            this.transferEncoding = transferEncoding;
        }

        public string FileName
        {
            get { return fileName; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] FileData
        {
            get { return fileData; }
        }

        public string TransferEncoding
        {
            get { return transferEncoding; }
        }

        private readonly string fileName;
        private readonly byte[] fileData;
        private readonly string transferEncoding;
    }
}