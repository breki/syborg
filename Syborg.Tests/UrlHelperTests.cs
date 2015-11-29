using NUnit.Framework;
using Syborg.Razor;

namespace Syborg.Tests
{
    public class UrlHelperTests
    {
        [Test]
        public void ApplicationWithinVirtualDir()
        {
            UrlHelper url = new UrlHelper ("http://localhost/wagahaga");
            Assert.AreEqual("/wagahaga/", url.Base());
        } 

        [Test]
        public void ApplicationOnRoot()
        {
            UrlHelper url = new UrlHelper ("http://wagahaga.com");
            Assert.AreEqual("/", url.Base());
        } 
    }
}