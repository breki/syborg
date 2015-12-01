using System.Web;
using LibroLib.FileSystem;
using NUnit.Framework;
using Syborg.Razor;

namespace Syborg.Tests
{
    public class RazorViewExecutionTests
    {
        [Test]
        public void EnsureSectionWithinSameTemplateIsRendered()
        {
            engine.RegisterView("Sample");
            ICompiledRazorTemplate template = engine.GetViewTemplateByName("Sample");
            RazorEngineExecutionSettings executionSettings = new RazorEngineExecutionSettings();
            string result = template.Execute(executionSettings);

            //if (DebugMode)
            //    Console.Out.WriteLine(result);

            StringAssert.Contains ("<div>This is a sample section</div>", result);
        }

        [SetUp]
        public void Setup()
        {
            IFileSystem fileSystem = new WindowsFileSystem();
            IRazorCompiler razorCompiler = new InMemoryRazorCompiler();
            engine = new RazorViewRenderingEngine (fileSystem, razorCompiler);

            RazorEngineCompileSettings razorEngineCompileSettings = new RazorEngineCompileSettings ();
            razorEngineCompileSettings.DefaultNamespace = "Syborg.Tests";
            razorEngineCompileSettings.DefaultClassName = "SyborgTestRazorTemplate";
            razorEngineCompileSettings.NamespaceImports.Add ("System");
            razorEngineCompileSettings.NamespaceImports.Add ("System.Collections");
            razorEngineCompileSettings.NamespaceImports.Add ("System.Collections.Generic");
            razorEngineCompileSettings.DefaultBaseClass = typeof(RazorTemplateBase).FullName;
            razorEngineCompileSettings.ReferenceAssemblies.Add (typeof(HtmlString).Assembly);
            razorEngineCompileSettings.DebugMode = DebugMode;

            engine.Initialize ("Views", razorEngineCompileSettings);
        }

        private RazorViewRenderingEngine engine;
        private const bool DebugMode = false;
    }
}