using System;
using LibroLib;

namespace Syborg.Policies
{
    public class HstsPolicy : IWebPolicy
    {
        public HstsPolicy()
        {
            maxAge = TimeSpan.FromDays(365);
        }

        public HstsPolicy(TimeSpan maxAge)
        {
            this.maxAge = maxAge;
        }

        public void Apply(IWebContext context)
        {
            if (!context.Configuration.UseHsts)
                return;

            context.RemoveHeader(HttpConsts.HeaderStrictTransportSecurity);

            string headerValue =
                "max-age={0}".Fmt((int)Math.Round(maxAge.TotalSeconds, 0));
            context.AddHeader(
                HttpConsts.HeaderStrictTransportSecurity,
                headerValue);
        }

        private readonly TimeSpan maxAge;
    }
}