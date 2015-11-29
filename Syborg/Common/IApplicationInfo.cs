using System;

namespace Syborg.Common
{
    public interface IApplicationInfo
    {
        string AppRootDirectory { get; }

        Version AppVersion { get; }
        string AppVersionString { get; }

        bool IsMono { get; }
        string MonoVersion { get; }

        bool Is64Bit { get; }

        long MemoryUsed { get; }
        long GCTotalMemory { get; }

        string GetAppDirectoryPath (string subpath);
    }
}