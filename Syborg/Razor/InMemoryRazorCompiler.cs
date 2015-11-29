using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Web.Razor;
using System.Web.Razor.Generator;
using log4net;
using Microsoft.CSharp;

namespace Syborg.Razor
{
    public class InMemoryRazorCompiler : IRazorCompiler
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public ICompiledRazorTemplate Compile (string razorTemplateText, RazorEngineCompileSettings settings, string templateName = null)
        {
            RazorEngineHost razorEngineHost = new RazorEngineHost (new CSharpRazorCodeLanguage ());
            razorEngineHost.DefaultNamespace = settings.DefaultNamespace;
            razorEngineHost.DefaultClassName = settings.DefaultClassName;
            razorEngineHost.DefaultBaseClass = settings.DefaultBaseClass;
            razorEngineHost.GeneratedClassContext = new GeneratedClassContext (
                GeneratedClassContext.DefaultExecuteMethodName,
                GeneratedClassContext.DefaultWriteMethodName,
                GeneratedClassContext.DefaultWriteLiteralMethodName,
                "WriteTo",
                "WriteLiteralTo",
                "Syborg.Razor.HelperResult",
                "DefineSection");

            foreach (string namespaceImport in settings.NamespaceImports)
                razorEngineHost.NamespaceImports.Add (namespaceImport);

            RazorTemplateEngine razorTemplateEngine = new RazorTemplateEngine (razorEngineHost);

            using (StringReader templateReader = new StringReader (razorTemplateText))
            {
                GeneratorResults generatorResults = razorTemplateEngine.GenerateCode(templateReader);
                if (!generatorResults.Success)
                    throw new RazorException(generatorResults);

                // do this only for debug purposes
                if (templateName != null && settings.DebugMode)
                {
                    lock (log)
                    {
                        string generatedTemplateFileName = Path.Combine(Path.GetTempPath(), templateName + ".cs");
                        using (StreamWriter sourceCodeWriter = new StreamWriter(generatedTemplateFileName))
                        using (CSharpCodeProvider provider = new CSharpCodeProvider ())
                        {
                            CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
                            provider.GenerateCodeFromCompileUnit(generatorResults.GeneratedCode, sourceCodeWriter, codeGeneratorOptions);
                            if (log.IsDebugEnabled)
                                log.DebugFormat("Writing the generated template to '{0}", generatedTemplateFileName);
                        }
                    }
                }

                CompilerParameters compilerParameters = new CompilerParameters();
                compilerParameters.GenerateInMemory = true;
                compilerParameters.IncludeDebugInformation = true;
                compilerParameters.ReferencedAssemblies.Add(typeof(InMemoryRazorCompiler).Assembly.Location);
                compilerParameters.TreatWarningsAsErrors = true;
                foreach (Assembly referenceAssembly in settings.ReferenceAssemblies)
                    compilerParameters.ReferencedAssemblies.Add(referenceAssembly.Location);

                using (CSharpCodeProvider codeProvider = new CSharpCodeProvider())
                {
                    CompilerResults compilerResults = codeProvider.CompileAssemblyFromDom(compilerParameters, generatorResults.GeneratedCode);

                    if (compilerResults.Errors.HasErrors)
                        throw new RazorException(generatorResults, compilerResults);

                    return new CompiledRazorTemplate(generatorResults, compilerResults);
                }
            }
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}