using System;

namespace Syborg.Caching
{
    public class NoCachingPolicy : ICachingPolicy
    {
        public void ProcessRequest (object resourceData, IWebContext context, Action<object, IWebContext> returnResourceAction)
        {
            context.AddHeader(HttpConsts.HeaderDate, context.TimeService.CurrentTime.ToRfc2822DateTime());
            context.ResponseHeaders.Remove (HttpConsts.HeaderCacheControl);
            context.AddHeader (HttpConsts.HeaderCacheControl, HttpConsts.CacheControlNoCache);
            context.AddHeader(HttpConsts.HeaderCacheControl, HttpConsts.CacheControlNoStore);
            context.AddHeader(HttpConsts.HeaderCacheControl, HttpConsts.CacheControlMaxAge + "=0");
            context.AddHeader (HttpConsts.HeaderExpires, new DateTime (2000, 01, 01).ToRfc2822DateTime ());

            returnResourceAction(resourceData, context);
        }
    }
}