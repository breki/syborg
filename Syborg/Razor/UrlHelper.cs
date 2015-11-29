using System;
using System.Diagnostics.Contracts;

namespace Syborg.Razor
{
    public class UrlHelper
    {
        public UrlHelper(string applicationUrl)
        {
            Contract.Requires(applicationUrl != null);

            applicationAbsoluteBaseUrl = applicationUrl;
            if (!applicationAbsoluteBaseUrl.EndsWith("/", StringComparison.Ordinal))
                applicationAbsoluteBaseUrl += "/";

            Uri applicationPathUri = new Uri(applicationUrl);
            string path = applicationPathUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);

            if (path.Length == 0)
                applicationRelativeBaseUrl = "/";
            else
                applicationRelativeBaseUrl = "/" + path + "/";
        }

        public string Base ()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return applicationRelativeBaseUrl;
        }

        public string AbsoluteBase()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return applicationAbsoluteBaseUrl;
        }

        private readonly string applicationRelativeBaseUrl;
        private readonly string applicationAbsoluteBaseUrl;
    }
}