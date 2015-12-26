using System;
using System.Globalization;
using NUnit.Framework;

namespace Syborg.Tests
{
    public class WebServerExtensionsTests
    {
        [Test]
        public void ToRfc2822()
        {
            DateTime time = new DateTime (2014, 06, 15, 18, 14, 00, 123, DateTimeKind.Utc);
            Assert.AreEqual ("Sun, 15 Jun 2014 18:14:00 GMT", time.ToRfc2822DateTime ());
        } 

        [Test]
        [TestCase ("Sun, 15 Jun 2014 16:14:55 GMT", "15-06-2014 16:14:55")]
        [TestCase ("15 Jun 2014 16:14:55 GMT", "15-06-2014 16:14:55")]
        [TestCase ("15 Jun 2014 16:14 GMT", "15-06-2014 16:14:00")]
        [TestCase ("whatever", null)]
        public void FromRfc2822(string rfcTime, string expectedTimeString)
        {
            DateTime? parsedTime = WebServerExtensions.FromRfc2822DateTime (rfcTime);

            if (expectedTimeString == null)
                Assert.IsNull(parsedTime);
            else
            {
                DateTime expectedTime = DateTime.ParseExact(
                    expectedTimeString, 
                    "dd-MM-yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal);
                Assert.AreEqual(expectedTime, parsedTime);
            }
        } 
    }
}