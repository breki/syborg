using System.Diagnostics.Contracts;

namespace Syborg
{
    [ContractClass(typeof(IFileMimeTypesMapContract))]
    public interface IFileMimeTypesMap
    {
        void RegisterMimeType(string fileExtension, string mimeType);
        string GetContentType(string fileName);
    }

    [ContractClassFor(typeof(IFileMimeTypesMap))]
    internal abstract class IFileMimeTypesMapContract : IFileMimeTypesMap
    {
        public void RegisterMimeType(string fileExtension, string mimeType)
        {
            Contract.Requires(fileExtension != null);
            Contract.Requires(mimeType != null);
        }

        public string GetContentType(string fileName)
        {
            Contract.Requires(fileName != null);
            throw new System.NotImplementedException();
        }
    }
}