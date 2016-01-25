using System;
using System.Diagnostics.Contracts;

namespace Syborg.Commands
{
    /// <summary>
    /// Provides methods to access the content files.
    /// </summary>
    [ContractClass(typeof(IContentRepositoryContract))]
    public interface IContentRepository
    {
        /// <summary>
        /// Checks whether the specified content file exists on the disk.
        /// </summary>
        /// <param name="contentFilePath">The path of the content file.</param>
        /// <returns><c>true</c> if the file exists; <c>false</c> otherwise</returns>
        bool DoesFileExist(string contentFilePath);
    }

    [ContractClassFor(typeof(IContentRepository))]
    // ReSharper disable once InconsistentNaming
    internal abstract class IContentRepositoryContract : IContentRepository
    {
        bool IContentRepository.DoesFileExist(string contentFilePath)
        {
            Contract.Requires(contentFilePath != null);
            throw new InvalidOperationException();
        }
    }
}