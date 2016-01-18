using NUnit.Framework;
using Syborg.ContentHandling;

namespace Syborg.Tests
{
    public class FileCacheTests
    {
        [Test]
        public void FileIsNotCached ()
        {
            CachableFileInfo fileInfo;
            Assert.IsFalse(fileCache.TryGetFile("somefile", null, out fileInfo));
        }

        [Test]
        public void CacheFile ()
        {
            const string SampleFileName = "somefile";
            const string SampleEncoding = "encoding";
            byte[] sampleFileData = CreateSampleFileData();

            fileCache.CacheFile(SampleFileName, sampleFileData, SampleEncoding);
            CachableFileInfo fileInfo;
            Assert.IsTrue(fileCache.TryGetFile(SampleFileName, SampleEncoding, out fileInfo));

            Assert.AreEqual(SampleFileName, fileInfo.FileName);
            Assert.AreEqual(SampleEncoding, fileInfo.TransferEncoding);
            CollectionAssert.AreEqual(sampleFileData, fileInfo.FileData);
        }

        [Test]
        public void CacheSameFileWithDifferentTransferEncodings()
        {
            const string SampleFileName = "somefile";
            const string SampleEncoding = "encoding";
            byte[] sampleFileData = CreateSampleFileData ();

            fileCache.CacheFile (SampleFileName, sampleFileData);
            fileCache.CacheFile (SampleFileName, sampleFileData, SampleEncoding);

            CachableFileInfo fileInfo;
            Assert.IsTrue (fileCache.TryGetFile (SampleFileName, null, out fileInfo));

            Assert.AreEqual (SampleFileName, fileInfo.FileName);
            Assert.AreEqual (null, fileInfo.TransferEncoding);
            CollectionAssert.AreEqual (sampleFileData, fileInfo.FileData);

            Assert.IsTrue (fileCache.TryGetFile (SampleFileName, SampleEncoding, out fileInfo));

            Assert.AreEqual (SampleFileName, fileInfo.FileName);
            Assert.AreEqual (SampleEncoding, fileInfo.TransferEncoding);
            CollectionAssert.AreEqual (sampleFileData, fileInfo.FileData);
        }

        [SetUp]
        public void Setup()
        {
            fileCache = new FileCache();
        }

        private static byte[] CreateSampleFileData()
        {
            byte[] data = new byte[10];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)i;

            return data;
        }

        private FileCache fileCache;
    }
}