using System.Diagnostics.Contracts;

namespace Syborg.Razor
{
    /// <summary>
    /// Engine for rendering views using Razor templating language, with layout support.
    /// </summary>
    [ContractClass(typeof(IRazorViewRenderingEngineContract))]
    public interface IRazorViewRenderingEngine
    {
        void Initialize(string viewsRootDirectory, RazorEngineCompileSettings settings);
        void RegisterLayout (string layoutName);
        void RegisterView<TModel>(string layoutName = null, string nspace = null);
        void RegisterView(string viewName, string layoutName = null, string nspace = null);
        ICompiledRazorTemplate GetViewTemplateByModel<TModel>();
        ICompiledRazorTemplate GetViewTemplateByName(string viewName);
    }

    [ContractClassFor(typeof(IRazorViewRenderingEngine))]
    internal abstract class IRazorViewRenderingEngineContract : IRazorViewRenderingEngine
    {
        public void Initialize (string viewsRootDirectory, RazorEngineCompileSettings settings)
        {
            Contract.Requires(viewsRootDirectory != null);
            Contract.Requires(settings != null);
            throw new System.NotImplementedException();
        }

        public void RegisterLayout(string layoutName)
        {
            Contract.Requires(layoutName != null);
            throw new System.NotImplementedException();
        }

        public void RegisterView<TModel>(string layoutName = null, string nspace = null)
        {
            throw new System.NotImplementedException();
        }

        public void RegisterView(string viewName, string layoutName = null, string nspace = null)
        {
            Contract.Requires(viewName != null);
            throw new System.NotImplementedException();
        }

        public ICompiledRazorTemplate GetViewTemplateByModel<TModel>()
        {
            Contract.Ensures(Contract.Result<ICompiledRazorTemplate>() != null);
            throw new System.NotImplementedException();
        }

        public ICompiledRazorTemplate GetViewTemplateByName(string viewName)
        {
            Contract.Requires(viewName != null);
            Contract.Ensures(Contract.Result<ICompiledRazorTemplate>() != null);
            throw new System.NotImplementedException();
        }
    }
}