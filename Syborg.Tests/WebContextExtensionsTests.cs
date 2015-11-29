using System.Collections.Specialized;
using NUnit.Framework;

namespace Syborg.Tests
{
    public class WebContextExtensionsTests
    {
        [Test]
        public void DumpRequestHeaders()
        {
            FakeWebContext context = new FakeWebContext(null, null, null, null, null);
            context.RequestHeaders.Add("h1", "v1");
            context.RequestHeaders.Add("h1", "v2");
            context.RequestHeaders.Add("h2", "v3");

            string dump = context.DumpRequestHeaders();
            Assert.AreEqual("h1='v1','v2'; h2='v3'", dump);
        }

        [Test]
        public void ExtractFormData()
        {
            FakeWebContext context = new FakeWebContext (null, null, null, null, null);
            context.RequestHeaders.Add (HttpConsts.HeaderContentType, HttpConsts.ContentTypeApplicationXWwwFormUrlencoded);
            context.SetRequestBody("ContactEmail=wagahaga%40gmail.com&Message=test&MailPurpose=Contact&AdditionalData1=");

            NameValueCollection formData = context.ExtractFormData();
            Assert.AreEqual ("wagahaga@gmail.com", formData["ContactEmail"]);
            Assert.AreEqual("test", formData["Message"]);
            Assert.AreEqual("Contact", formData["MailPurpose"]);
            Assert.AreEqual(string.Empty, formData["AdditionalData1"]);
        }

        [Test]
        public void EncodeFormData()
        {
            const string Message = @"Hello,

I need some maps fast!";
            
            NameValueCollection formData = new NameValueCollection();
            formData.Add ("ContactEmail", "wagahaga@gmail.com");
            formData.Add("MailPurpose", "Contact");
            formData.Add(
                "Message", 
Message);

            FakeWebContext context = new FakeWebContext (null, null, null, null, null);
            context.EncodeFormDataToRequest (formData);

            Assert.AreEqual (
                "ContactEmail=wagahaga%40gmail.com&MailPurpose=Contact&Message=Hello%2c%0d%0a%0d%0aI+need+some+maps+fast!", 
                context.ReadRequestStreamAsText());
        }
    }
}