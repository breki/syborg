namespace Syborg.Policies
{
    public class SecureResponseHeadersPolicy : IWebPolicy
    {
        public void Apply(IWebContext context)
        {
            context.RemoveHeader(HttpConsts.HeaderServer);
            context.AddHeader(HttpConsts.HeaderXFrameOptions, "SAMEORIGIN");
            context.AddHeader(HttpConsts.HeaderXXssProtection, "1; mode=block");
            context.AddHeader(HttpConsts.HeaderXContentTypeOptions, "nosniff");
        }
    }
}