using System.Diagnostics.Contracts;

namespace Syborg.Razor
{
    [ContractClass(typeof(ICompiledRazorTemplateContract))]
    public interface ICompiledRazorTemplate
    {
        object CreateTemplateInstance();
        string Execute(RazorEngineExecutionSettings executionSettings);
        string ExecuteWithLayout (ICompiledRazorTemplate innerTemplate, RazorEngineExecutionSettings executionSettings);
        ICompiledRazorTemplate LayoutTemplate { get; set; }
    }

    [ContractClassFor(typeof(ICompiledRazorTemplate))]
    internal abstract class ICompiledRazorTemplateContract : ICompiledRazorTemplate
    {
        public object CreateTemplateInstance()
        {
            Contract.Ensures(Contract.Result<object>() != null);
            throw new System.NotImplementedException();
        }

        public string Execute(RazorEngineExecutionSettings executionSettings)
        {
            Contract.Requires(executionSettings != null);
            Contract.Ensures(Contract.Result<string>() != null);
            throw new System.NotImplementedException();
        }

        public string ExecuteWithLayout(ICompiledRazorTemplate innerTemplate, RazorEngineExecutionSettings executionSettings)
        {
            Contract.Requires(innerTemplate != null);
            Contract.Requires(executionSettings != null);
            Contract.Ensures(Contract.Result<string>() != null);
            throw new System.NotImplementedException();
        }

        public ICompiledRazorTemplate LayoutTemplate
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }
    }
}