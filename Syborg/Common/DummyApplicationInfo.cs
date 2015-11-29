using System;
using System.IO;

namespace Syborg.Common
{
    public class DummyApplicationInfo : IApplicationInfo
    {
        public DummyApplicationInfo (string applicationRootDirectory)
        {
            this.applicationRootDirectory = applicationRootDirectory;
        }

        public string AppRootDirectory
        {
            get { return applicationRootDirectory; }
        }

        public Version AppVersion
        {
            get { return new Version (1, 2); }
        }

        public string AppVersionString
        {
            get { return "1.2"; }
        }

        public bool IsMono
        {
            get { return false; }
        }

        public string MonoVersion
        {
            get { throw new NotSupportedException (); }
        }

        public bool Is64Bit
        {
            get { return true; }
        }

        public long MemoryUsed
        {
            get { return 0; }
        }

        public long GCTotalMemory
        {
            get { return 0; }
        }

        public string GetAppDirectoryPath (string subpath)
        {
            return Path.Combine (applicationRootDirectory, subpath);
        }

        private readonly string applicationRootDirectory;
    }
}