namespace Syborg.Policies
{
    public class HstsPolicy : IWebPolicy
    {
        public void Apply(IWebContext context)
        {
            if (!context.Configuration.UseHsts)
                return;

            // note: this means 1 year
            context.RemoveHeader(HttpConsts.HeaderStrictTransportSecurity);
            context.AddHeader(HttpConsts.HeaderStrictTransportSecurity, "max-age=31536000");
        }
    }
}