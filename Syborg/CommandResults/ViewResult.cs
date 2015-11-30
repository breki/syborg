using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using LibroLib;
using Syborg.Caching;
using Syborg.Common;
using Syborg.Razor;

namespace Syborg.CommandResults
{
    public class ViewResult<TModel> : WebCommandResultBase
    {
        public ViewResult (TModel model, RazorEngineExecutionSettings razorEngineExecutionSettings)
        {
            Contract.Requires(model != null);
            this.model = model;
            this.razorEngineExecutionSettings = razorEngineExecutionSettings;
        }

        public ViewResult (string viewName, TModel model, RazorEngineExecutionSettings razorEngineExecutionSettings)
        {
            Contract.Requires(model != null);
 
            this.model = model;
            this.razorEngineExecutionSettings = razorEngineExecutionSettings;
            SetExplicitViewName(viewName);
        }

        public TModel Model
        {
            get
            {
                Contract.Ensures(Contract.Result<TModel>() != null);
                return model;
            }
        }

        public override void Apply (IWebContext context)
        {
            ApplyEssentials(context);
            RenderView (context);
        }

        public void SetRazorEngineExecutionSettings(RazorEngineExecutionSettings settings)
        {
            Contract.Requires(settings != null);

            razorEngineExecutionSettings = settings;
        }

        protected ViewResult(TModel model)
        {
            Contract.Requires(model != null);

            this.model = model;
        }

        protected void SetExplicitViewName(string viewName)
        {
            explicitViewName = viewName;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(model != null);
        }

        private void RenderView (IWebContext context)
        {
            ICachingPolicy cachingPolicy = new NoCachingPolicy();
            cachingPolicy.ProcessRequest(null, context, ReturnView);
        }

        private void ReturnView(object arg1, IWebContext context)
        {
            // fetch the view template
            ICompiledRazorTemplate viewTemplate;

            if (explicitViewName == null)
                viewTemplate = context.ViewRenderingEngine.GetViewTemplateByModel<TModel>();
            else
                viewTemplate = context.ViewRenderingEngine.GetViewTemplateByName (explicitViewName);

            razorEngineExecutionSettings.Properties.Add ("Model", model);
            razorEngineExecutionSettings.Properties.Add ("WebContext", context);
            string contents = viewTemplate.Execute (razorEngineExecutionSettings);

            using (StreamWriter writer = new StreamWriter (context.ResponseStream))
                writer.Write (contents);

            context.StatusCode = (int)HttpStatusCode.OK;
            context.ResponseContentType = HttpConsts.ContentTypeTextHtmlUtf8;

            if (explicitViewName != null)
                context.ResponseDescription = "Returning view '{0}'".Fmt (explicitViewName);
            else
                context.ResponseDescription = "Returning view (model: {0})".Fmt (model.GetType ().FullName);

            context.ApplyPolicies ();
            context.CloseResponse ();
        }

        private readonly TModel model;
        private RazorEngineExecutionSettings razorEngineExecutionSettings;
        private string explicitViewName;
    }
}