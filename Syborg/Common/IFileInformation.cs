using System.IO;

namespace Syborg.Common
{
    public interface IFileInformation : IFileEntryInformation
    {
        long Length { get; }
        bool Exists { get; }
        FileAttributes Attributes { get; set; }

        void CopyTo (string destFileName, bool overwrite);
        void WriteToStream (Stream stream, byte[] buffer);
    }
}