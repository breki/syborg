using System;
using System.IO;

namespace Syborg.Common
{
    public class WindowsDirectoryInformation : IDirectoryInformation
    {
        public WindowsDirectoryInformation (DirectoryInfo dirInfo)
        {
            this.dirInfo = dirInfo;
        }

        public string FullName
        {
            get { return dirInfo.FullName; }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return dirInfo.LastWriteTimeUtc; }
            set { dirInfo.LastAccessTimeUtc = value; }
        }

        public DateTime LastWriteTime
        {
            get { return dirInfo.LastWriteTime; }
            set { dirInfo.LastAccessTime = value; }
        }

        public DateTime CreationTimeUtc
        {
            get { return dirInfo.CreationTimeUtc; }
            set { dirInfo.CreationTimeUtc = value; }
        }

        private readonly DirectoryInfo dirInfo;
    }
}