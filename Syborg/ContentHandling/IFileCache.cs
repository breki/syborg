using System.Diagnostics.Contracts;

namespace Syborg.ContentHandling
{
    [ContractClass(typeof(IFileCacheContract))]
    public interface IFileCache
    {
        void CacheFile(string fileName, byte[] fileData, string transferEncoding = null);
        bool TryGetFile(string fileName, string transferEncoding, out CachableFileInfo fileInfo);
    }

    [ContractClassFor(typeof(IFileCache))]
    // ReSharper disable once InconsistentNaming
    internal abstract class IFileCacheContract : IFileCache
    {
        void IFileCache.CacheFile(string fileName, byte[] fileData, string transferEncoding)
        {
            Contract.Requires (fileName != null);
            Contract.Requires (fileData != null);
            throw new System.NotImplementedException ();
        }

        bool IFileCache.TryGetFile(string fileName, string transferEncoding, out CachableFileInfo fileInfo)
        {
            Contract.Requires(fileName != null);
            Contract.Ensures(
                (Contract.ValueAtReturn(out fileInfo) != null && Contract.Result<bool>() == true)
                || (Contract.ValueAtReturn (out fileInfo) == null && Contract.Result<bool>() == false));
            throw new System.NotImplementedException();
        }
    }
}