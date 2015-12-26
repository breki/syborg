using System;
using System.Globalization;

namespace Syborg
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
    public static class WebServerExtensions
    {
        public static string ToRfc2822DateTime (this DateTime dateTime)
        {
            return dateTime.ToUniversalTime ().ToString ("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);
        }

        public static DateTime? FromRfc2822DateTime(string value)
        {
            DateTime result;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
                return result;

            return null;
        }
    }
}