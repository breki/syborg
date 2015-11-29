using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Xml;

namespace Syborg.Common
{
    /// <summary>
    /// An interface to the physical file system.
    /// </summary>
    [ContractClass (typeof(IFileSystemContract))]
    public interface IFileSystem
    {
        bool IsNetworkAvailable { get; }

        void CopyFile (string sourceFileName, string destinationFileName);
        void CreateDirectory (string dirPath, bool deleteExisting);
        void DeleteDirectory (string dirPath);
        void DeleteFile (string path, bool failIfNotExist);
        object DeserializeObjectFromXmlFile (string filePath, Type objectType, string xmlNamespace);
        object DeserializeObjectFromXmlFile (string filePath, Type objectType, string xmlNamespace, string xsdFilePath);
        bool DoesDirectoryExist (string path);
        bool DoesFileExist (string path);
        void EnsureDirectoryExists (string directory);
        IDirectoryInformation GetDirectoryInformation (string dirPath);
        IFileInformation[] GetDirectoryFiles (string dirPath);
        IFileInformation[] GetDirectoryFiles (string dirPath, string searchPattern);
        IDirectoryInformation[] GetDirectorySubdirectories (string dirPath);
        IFileInformation GetFileInformation (string filePath);
        bool IsDriveReady (string drive);
        void MoveFile (string sourceFileName, string destinationFileName);
        Stream OpenFile (
            string fileName,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            int bufferSize,
            FileOptions fileOptions);
        Stream OpenFileToRead (string fileName);
        Stream OpenFileToRead (string fileName, FileOptions fileOptions);
        Stream OpenFileToWrite (string fileName);
        Stream OpenFileToWrite (string fileName, FileOptions fileOptions);
        byte[] ReadFileAsBytes (string fileName);
        string ReadFileAsString (string fileName);
        [SuppressMessage ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        XmlDocument ReadFileAsXmlDocument (string fileName);
        IEnumerable<string> ReadFileAsStringLines (string fileName);
        [SuppressMessage ("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj")]
        void SerializeObjectIntoXmlFile (string filePath, object obj, string xmlStyleSheetProcessingInstructions);
        void WriteFile (string fileName, byte[] data);
        void WriteFile (string fileName, string contents, Encoding encoding);
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        void WriteXmlDocument (string fileName, XmlDocument xmlDoc);
    }

    [ContractClassFor (typeof(IFileSystem))]
    internal abstract class IFileSystemContract : IFileSystem
    {
        public bool IsNetworkAvailable
        {
            get { throw new NotImplementedException (); }
        }

        public void CopyFile (string sourceFileName, string destinationFileName)
        {
            Contract.Requires (sourceFileName != null);
            Contract.Requires (destinationFileName != null);
            throw new NotImplementedException ();
        }

        public void CreateDirectory (string dirPath, bool deleteExisting)
        {
            Contract.Requires (dirPath != null);
            throw new NotImplementedException ();
        }

        public void DeleteDirectory (string dirPath)
        {
            Contract.Requires (dirPath != null);
            throw new NotImplementedException ();
        }

        public void DeleteFile (string path, bool failIfNotExist)
        {
            Contract.Requires (path != null);
            throw new NotImplementedException ();
        }

        public object DeserializeObjectFromXmlFile (string filePath, Type objectType, string xmlNamespace)
        {
            Contract.Requires (filePath != null);
            Contract.Requires (objectType != null);
            throw new NotImplementedException ();
        }

        public object DeserializeObjectFromXmlFile (string filePath, Type objectType, string xmlNamespace, string xsdFilePath)
        {
            Contract.Requires (filePath != null);
            Contract.Requires (objectType != null);
            throw new NotImplementedException ();
        }

        public bool DoesDirectoryExist (string path)
        {
            Contract.Requires (path != null);
            throw new NotImplementedException ();
        }

        public bool DoesFileExist (string path)
        {
            Contract.Requires (path != null);
            throw new NotImplementedException ();
        }

        public void EnsureDirectoryExists (string directory)
        {
            Contract.Requires (directory != null);
            throw new NotImplementedException ();
        }

        public IDirectoryInformation GetDirectoryInformation (string dirPath)
        {
            Contract.Requires (dirPath != null);
            Contract.Ensures (Contract.Result<IDirectoryInformation>() != null);
            throw new NotImplementedException ();
        }

        public IFileInformation[] GetDirectoryFiles (string dirPath)
        {
            Contract.Requires (dirPath != null);
            Contract.Ensures (Contract.Result<IFileInformation[]>() != null);
            Contract.Ensures (Contract.ForAll (Contract.Result<IFileInformation[]>(), x => x != null));
            return default(IFileInformation[]);
        }

        public IFileInformation[] GetDirectoryFiles (string dirPath, string searchPattern)
        {
            Contract.Requires (dirPath != null);
            Contract.Ensures (Contract.Result<IFileInformation[]>() != null);
            Contract.Ensures (Contract.ForAll (Contract.Result<IFileInformation[]>(), x => x != null));
            return default(IFileInformation[]);
        }

        public IDirectoryInformation[] GetDirectorySubdirectories (string dirPath)
        {
            Contract.Requires (dirPath != null);
            Contract.Ensures (Contract.Result<IDirectoryInformation[]>() != null);
            Contract.Ensures (Contract.ForAll (Contract.Result<IDirectoryInformation[]>(), x => x != null));
            return default(IDirectoryInformation[]);
        }

        public IFileInformation GetFileInformation (string filePath)
        {
            Contract.Requires (filePath != null);
            Contract.Ensures (Contract.Result<IFileInformation>() != null);
            throw new NotImplementedException ();
        }

        public bool HasRights (string path, WindowsIdentity identity, FileSystemRights rights)
        {
            Contract.Requires (path != null);
            throw new NotImplementedException ();
        }

        public bool IsDriveReady (string drive)
        {
            throw new NotImplementedException ();
        }

        public void MoveFile (string sourceFileName, string destinationFileName)
        {
            Contract.Requires (sourceFileName != null);
            Contract.Requires (destinationFileName != null);
            throw new NotImplementedException ();
        }

        public Stream OpenFile (string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, int bufferSize, FileOptions fileOptions)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<Stream>() != null);
            throw new NotImplementedException ();
        }

        public Stream OpenFileToRead (string fileName)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<Stream>() != null);
            throw new NotImplementedException ();
        }

        public Stream OpenFileToRead (string fileName, FileOptions fileOptions)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<Stream>() != null);
            throw new NotImplementedException ();
        }

        public Stream OpenFileToWrite (string fileName)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<Stream>() != null);
            throw new NotImplementedException ();
        }

        public Stream OpenFileToWrite (string fileName, FileOptions fileOptions)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<Stream>() != null);
            throw new NotImplementedException ();
        }

        public byte[] ReadFileAsBytes (string fileName)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<byte[]>() != null);
            throw new NotImplementedException ();
        }

        public string ReadFileAsString (string fileName)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<string>() != null);
            throw new NotImplementedException ();
        }

        public XmlDocument ReadFileAsXmlDocument (string fileName)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<XmlDocument>() != null);
            throw new NotImplementedException ();
        }

        public IEnumerable<string> ReadFileAsStringLines (string fileName)
        {
            Contract.Requires (fileName != null);
            Contract.Ensures (Contract.Result<IEnumerable<string>>() != null);
            throw new NotImplementedException ();
        }

        public void SerializeObjectIntoXmlFile (string filePath, object obj, string xmlStyleSheetProcessingInstructions)
        {
            Contract.Requires (filePath != null);
            Contract.Requires (obj != null);
            throw new NotImplementedException ();
        }

        public void WriteFile (string fileName, byte[] data)
        {
            Contract.Requires (fileName != null);
            Contract.Requires (data != null);
            throw new NotImplementedException ();
        }

        public void WriteFile (string fileName, string contents, Encoding encoding)
        {
            Contract.Requires (fileName != null);
            Contract.Requires (contents != null);
            Contract.Requires (encoding != null);
            throw new NotImplementedException ();
        }

        public void WriteXmlDocument (string fileName, XmlDocument xmlDoc)
        {
            Contract.Requires (fileName != null);
            Contract.Requires (xmlDoc != null);
            throw new NotImplementedException ();
        }
    }
}