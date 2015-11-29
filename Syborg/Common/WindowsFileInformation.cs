using System;
using System.IO;

namespace Syborg.Common
{
    public class WindowsFileInformation : IFileInformation
    {
        public string FullName
        {
            get { return fileInfo.FullName; }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return fileInfo.LastWriteTimeUtc; }
            set { fileInfo.LastAccessTimeUtc = value; }
        }

        public DateTime LastWriteTime
        {
            get { return fileInfo.LastWriteTime; }
            set { fileInfo.LastAccessTime = value; }
        }

        public DateTime CreationTimeUtc
        {
            get { return fileInfo.CreationTimeUtc; }
            set { fileInfo.CreationTimeUtc = value; }
        }

        public bool Exists
        {
            get { return fileInfo.Exists; }
        }

        public long Length
        {
            get { return fileInfo.Length; }
        }

        public FileAttributes Attributes
        {
            get
            {
                return fileInfo.Attributes;
            }

            set
            {
                fileInfo.Attributes = value;
            }
        }

        public WindowsFileInformation (FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public void CopyTo (string destFileName, bool overwrite)
        {
            fileInfo.CopyTo (destFileName, overwrite);
        }

        public void WriteToStream (Stream stream, byte[] buffer)
        {
            if (stream == null)
                throw new ArgumentNullException ("stream");

            if (buffer == null)
                throw new ArgumentNullException ("buffer");

            using (FileStream fs = this.fileInfo.OpenRead ())
            {
                // Using a fixed size buffer here makes no noticeable difference for output
                // but keeps a lid on memory usage.
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read (buffer, 0, buffer.Length);
                    stream.Write (buffer, 0, sourceBytes);
                }
                while (sourceBytes > 0);
            }
        }

        private readonly FileInfo fileInfo;
    }
}