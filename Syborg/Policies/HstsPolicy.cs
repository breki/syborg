using System.Reflection;
using log4net;

namespace Syborg.Policies
{
    public class HstsPolicy : IWebPolicy
    {
        public void Apply(IWebContext context)
        {
            if (!context.Configuration.UseHsts)
                return;

            // note: this means 1 year
            context.ResponseHeaders.Remove(HttpConsts.HeaderStrictTransportSecurity);
            context.AddHeader(HttpConsts.HeaderStrictTransportSecurity, "max-age=31536000");
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}