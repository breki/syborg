using System.Diagnostics.Contracts;

namespace Syborg.Razor
{
    [ContractClass(typeof(IRazorCompilerContract))]
    public interface IRazorCompiler
    {
        ICompiledRazorTemplate Compile(string razorTemplateText, RazorEngineCompileSettings settings, string templateName = null);
    }

    [ContractClassFor(typeof(IRazorCompiler))]
    internal abstract class IRazorCompilerContract : IRazorCompiler
    {
        public ICompiledRazorTemplate Compile(string razorTemplateText, RazorEngineCompileSettings settings, string templateName = null)
        {
            Contract.Requires(razorTemplateText != null);
            Contract.Requires(settings != null);
            Contract.Ensures(Contract.Result<ICompiledRazorTemplate>() != null);
            throw new System.NotImplementedException();
        }
    }
}