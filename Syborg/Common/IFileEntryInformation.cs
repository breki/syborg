using System;
using System.Diagnostics.Contracts;

namespace Syborg.Common
{
    [ContractClass (typeof(IFileEntryInformationContract))]
    public interface IFileEntryInformation
    {
        string FullName { get; }
        DateTime LastWriteTimeUtc { get; set; }
        DateTime LastWriteTime { get; set; }
        DateTime CreationTimeUtc { get; set; }
    }

    [ContractClassFor (typeof(IFileEntryInformation))]
    internal abstract class IFileEntryInformationContract : IFileEntryInformation
    {
        string IFileEntryInformation.FullName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new NotImplementedException();
            }
        }

        DateTime IFileEntryInformation.LastWriteTimeUtc
        {
            get { throw new NotImplementedException (); }
            set { throw new NotImplementedException (); }
        }

        DateTime IFileEntryInformation.LastWriteTime
        {
            get { throw new NotImplementedException (); }
            set { throw new NotImplementedException (); }
        }

        DateTime IFileEntryInformation.CreationTimeUtc
        {
            get { throw new NotImplementedException (); }
            set { throw new NotImplementedException (); }
        }
    }
}