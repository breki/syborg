using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace Syborg
{
    public static class WebContextExtensions
    {
        public static string DumpRequestHeaders(this IWebContext context)
        {
            Contract.Requires(context != null);
            Contract.Ensures(Contract.Result<string>() != null);

            StringBuilder s = new StringBuilder();

            string headerSeparator = null;
            foreach (string header in context.RequestHeaders)
            {
                s.AppendFormat(CultureInfo.InvariantCulture, "{1}{0}=", header, headerSeparator);
                headerSeparator = "; ";

                string valueComma = null;
                foreach (string value in context.RequestHeaders.GetValues(header))
                {
                    s.AppendFormat("{0}'{1}'", valueComma, value);
                    valueComma = ",";
                }
            }

            return s.ToString();
        }

        public static NameValueCollection ExtractFormData (this IWebContext context)
        {
            Contract.Requires(context != null);

            string contentType = context.RequestHeaders[HttpConsts.HeaderContentType];
            
            if (contentType != HttpConsts.ContentTypeApplicationXWwwFormUrlencoded)
                return null;

            using (MemoryStream stream = new MemoryStream())
            {
                context.RequestStream.CopyTo(stream);
                string formDataString = Encoding.UTF8.GetString(stream.ToArray());
                return HttpUtility.ParseQueryString(formDataString, Encoding.UTF8);
            }
        }

        public static string ReadRequestPostString (this IWebContext context)
        {
            Contract.Requires(context != null);
            Contract.Ensures(Contract.Result<string>() != null);

            using (StreamReader reader = new StreamReader (context.RequestStream, Encoding.UTF8))
                return reader.ReadToEnd ();
        }
    }
}