using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Syborg.Common
{
    public class WindowsFileSystem : IFileSystem
    {
        public bool IsNetworkAvailable
        {
            get
            {
                return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable ();
            }
        }

        public void CopyFile (string sourceFileName, string destinationFileName)
        {
            File.Copy (sourceFileName, destinationFileName, true);
        }

        public void CreateDirectory (string dirPath, bool deleteExisting)
        {
            DirectoryInfo dirInfo = new DirectoryInfo (dirPath);
            if (dirInfo.Exists)
            {
                if (deleteExisting)
                    dirInfo.Delete (true);
                else
                    return;
            }

            dirInfo.Create ();
        }

        public void DeleteDirectory (string dirPath)
        {
            if (Directory.Exists (dirPath))
            {
                try
                {
                    Directory.Delete (dirPath, true);
                }
                catch (IOException ex)
                {
                    throw new InvalidOperationException ("Could not delete directory '{0}'".Fmt (dirPath), ex);
                }
            }
        }

        public IFileInformation[] GetDirectoryFiles (string dirPath)
        {
            DirectoryInfo dir = new DirectoryInfo (dirPath);
            FileInfo[] fileInfos = dir.GetFiles ();
            IFileInformation[] winFileInfos = new IFileInformation[fileInfos.Length];

            for (int i = 0; i < fileInfos.Length; i++)
                winFileInfos[i] = new WindowsFileInformation (fileInfos[i]);

            return winFileInfos;
        }

        public IFileInformation[] GetDirectoryFiles (string dirPath, string searchPattern)
        {
            DirectoryInfo dir = new DirectoryInfo (dirPath);
            FileInfo[] fileInfos = dir.GetFiles (searchPattern);
            IFileInformation[] winFileInfos = new IFileInformation[fileInfos.Length];

            for (int i = 0; i < fileInfos.Length; i++)
                winFileInfos[i] = new WindowsFileInformation (fileInfos[i]);

            return winFileInfos;
        }

        public IDirectoryInformation[] GetDirectorySubdirectories (string dirPath)
        {
            DirectoryInfo dir = new DirectoryInfo (dirPath);
            DirectoryInfo[] dirInfos = dir.GetDirectories ();
            IDirectoryInformation[] winDirInfos = new IDirectoryInformation[dirInfos.Length];

            for (int i = 0; i < dirInfos.Length; i++)
                winDirInfos[i] = new WindowsDirectoryInformation (dirInfos[i]);

            return winDirInfos;
        }

        public IFileInformation GetFileInformation (string filePath)
        {
            return new WindowsFileInformation (new FileInfo (filePath));
        }

        public void EnsureDirectoryExists (string directory)
        {
            if (false == string.IsNullOrEmpty (directory))
                Directory.CreateDirectory (directory);
        }

        public IDirectoryInformation GetDirectoryInformation (string dirPath)
        {
            return new WindowsDirectoryInformation (new DirectoryInfo (dirPath));
        }

        public bool DoesFileExist (string path)
        {
            return File.Exists (path);
        }

        public bool DoesDirectoryExist (string path)
        {
            return Directory.Exists (path);
        }

        public void DeleteFile (string path, bool failIfNotExist)
        {
            if (failIfNotExist || File.Exists (path))
                File.Delete (path);
        }

        public object DeserializeObjectFromXmlFile (string filePath, Type objectType, string xmlNamespace)
        {
            XmlSerializer serializer = new XmlSerializer (objectType, xmlNamespace);

            using (FileStream stream = File.Open (filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return serializer.Deserialize (stream);
            }
        }

        public object DeserializeObjectFromXmlFile (
            string filePath,
            Type objectType,
            string xmlNamespace,
            string xsdFilePath)
        {
            XmlSerializer serializer = new XmlSerializer (objectType, xmlNamespace);

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings ();
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            xmlReaderSettings.Schemas.Add (xmlNamespace, xsdFilePath);

            using (FileStream stream = File.Open (filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                XmlReader validator = XmlReader.Create (stream, xmlReaderSettings);
                return serializer.Deserialize (validator);
            }
        }

        public void SerializeObjectIntoXmlFile (string filePath, object obj, string xmlStyleSheetProcessingInstructions)
        {
            using (FileStream stream = File.Open (filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                // define settings for the XML writer
                XmlWriterSettings writerSettings = new XmlWriterSettings ();
                writerSettings.Indent = true;
                writerSettings.IndentChars = "\t";
                writerSettings.NewLineOnAttributes = true;
                writerSettings.OmitXmlDeclaration = false;

                using (XmlWriter writer = XmlWriter.Create (stream, writerSettings))
                {
                    // add processing instruction for XSLT transformation
                    writer.WriteProcessingInstruction ("xml-stylesheet", xmlStyleSheetProcessingInstructions);

                    XmlSerializer serializer = new XmlSerializer (obj.GetType ());

                    serializer.Serialize (writer, obj);
                }
            }
        }

        public void WriteFile (string fileName, byte[] data)
        {
            File.WriteAllBytes (fileName, data);
        }

        public void WriteFile (string fileName, string contents, Encoding encoding)
        {
            File.WriteAllText (fileName, contents, encoding);
        }

        public void WriteXmlDocument (string fileName, XmlDocument xmlDoc)
        {
            xmlDoc.Save (fileName);
        }

        public void MoveFile (string sourceFileName, string destinationFileName)
        {
            File.Move (sourceFileName, destinationFileName);
        }

        public Stream OpenFile (
            string fileName,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            int bufferSize,
            FileOptions fileOptions)
        {
            return new FileStream (fileName, fileMode, fileAccess, fileShare, bufferSize, fileOptions);
        }

        public Stream OpenFileToRead (string fileName)
        {
            return File.Open (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public Stream OpenFileToRead (string fileName, FileOptions fileOptions)
        {
            return new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 128 * 1024, fileOptions);
        }

        public Stream OpenFileToWrite (string fileName)
        {
            return File.Open (fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        }

        public Stream OpenFileToWrite (string fileName, FileOptions fileOptions)
        {
            return new FileStream (fileName, FileMode.Create, FileAccess.Write, FileShare.None, 128 * 1024, fileOptions);
        }

        public byte[] ReadFileAsBytes (string fileName)
        {
            return File.ReadAllBytes (fileName);
        }

        public string ReadFileAsString (string fileName)
        {
            return File.ReadAllText (fileName);
        }

        public XmlDocument ReadFileAsXmlDocument (string fileName)
        {
            XmlDocument doc = new XmlDocument ();
            doc.Load (fileName);
            return doc;
        }

        public IEnumerable<string> ReadFileAsStringLines (string fileName)
        {
            return File.ReadAllLines (fileName);
        }

        public bool IsDriveReady (string drive)
        {
            DriveInfo driveInfo = new DriveInfo (drive);
            return driveInfo.IsReady;
        }
    }
}