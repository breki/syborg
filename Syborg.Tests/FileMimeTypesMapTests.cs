using System;
using NUnit.Framework;

namespace Syborg.Tests
{
    public class FileMimeTypesMapTests
    {
        [Test]
        public void MimeTypeNotRegistered()
        {
            Assert.IsNull(map.GetContentType("somewhere/else.txt"));
        }

        [Test]
        public void FileHasNoExtension()
        {
            Assert.IsNull(map.GetContentType("somewhere/else"));
        }

        [Test]
        public void MimeTypeIsRegistered()
        {
            map.RegisterMimeType(".txt", HttpConsts.ContentTypeTextPlain);
            Assert.AreEqual(HttpConsts.ContentTypeTextPlain, map.GetContentType("somewhere/else.txt"));
        }

        [Test]
        public void ExtensionsShouldBeCaseInsensitive()
        {
            map.RegisterMimeType(".txt", HttpConsts.ContentTypeTextPlain);
            Assert.AreEqual(HttpConsts.ContentTypeTextPlain, map.GetContentType("somewhere/else.TXT"));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void FileExtensionRegisteredTwice()
        {
            map.RegisterMimeType (".txt", HttpConsts.ContentTypeTextPlain);
            map.RegisterMimeType (".TXT", HttpConsts.ContentTypeTextPlain);
        }

        [SetUp]
        public void Setup()
        {
            map = new FileMimeTypesMap();
        }

        private FileMimeTypesMap map;
    }
}