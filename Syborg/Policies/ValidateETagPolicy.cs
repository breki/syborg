using System;
using System.Text.RegularExpressions;
using LibroLib;

namespace Syborg.Policies
{
    public class ValidateETagPolicy : IWebPolicy
    {
        public void Apply(IWebContext context)
        {
            string etag = context.ResponseHeaders[HttpConsts.HeaderETag];
            if (!string.IsNullOrEmpty (etag))
            {
                if (!etagRegex.IsMatch (etag))
                    throw new InvalidOperationException ("ETag '{0}' syntax isn't valid".Fmt (etag));
            }
        }

        private readonly Regex etagRegex = new Regex (@"^\""[^\\]*\""$", RegexOptions.Compiled);
    }
}