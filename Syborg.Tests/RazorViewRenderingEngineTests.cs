using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.Common;
using Syborg.Razor;

namespace Syborg.Tests
{
    public class RazorViewRenderingEngineTests
    {
        [Test]
        public void RegisterViewByName()
        {
            SimulateViewFile("TestView");
            engine.RegisterView ("TestView");

            ICompiledRazorTemplate template = engine.GetViewTemplateByName("TestView");
            Assert.IsNotNull(template);
        }

        [Test]
        public void RegisterViewByNameInSubfolder ()
        {
            SimulateViewFile("TestView", "admin");
            engine.RegisterView ("TestView", null, "admin");

            ICompiledRazorTemplate template = engine.GetViewTemplateByName ("TestView");
            Assert.IsNotNull (template);
        }

        [Test]
        public void RegisterViewByModelInSubfolder ()
        {
            SimulateViewFile ("Sample", "admin");
            engine.RegisterView<SampleViewModel>(null, "admin");

            ICompiledRazorTemplate template = engine.GetViewTemplateByName ("Sample");
            Assert.IsNotNull (template);
        }

        [Test]
        public void Test()
        {
            SimulateViewFile("SiteLayout");
            SimulateViewFile("Home");
            SimulateViewFile("Home2");

            engine.RegisterLayout("SiteLayout");
            engine.RegisterView("Home", "SiteLayout");
            engine.RegisterView("Home2", "SiteLayout");
        }

        [SetUp]
        public void Setup()
        {
            fileSystem = MockRepository.GenerateStub<IFileSystem>();

            razorCompiler = MockRepository.GenerateStub<IRazorCompiler>();
            razorCompiler.Stub(x => x.Compile(null, null)).IgnoreArguments().Return(new CompiledRazorTemplate(null, null));

            engine = new RazorViewRenderingEngine (fileSystem, razorCompiler);
            RazorEngineCompileSettings compilerSettings = new RazorEngineCompileSettings();
            engine.Initialize (ViewsDir, compilerSettings);
        }

        private void SimulateViewFile(string viewName, string nspace = null)
        {
            string viewFileName;

            if (nspace != null)
                viewFileName = Path.Combine (ViewsDir, nspace, "{0}.cshtml".Fmt(viewName));
            else
                viewFileName = Path.Combine (ViewsDir, "{0}.cshtml".Fmt (viewName));

            fileSystem.Stub (x => x.DoesFileExist (viewFileName)).Return (true);
            fileSystem.Stub (x => x.ReadFileAsString (viewFileName)).Return (string.Empty);
        }

        private RazorViewRenderingEngine engine;
        private IFileSystem fileSystem;
        private IRazorCompiler razorCompiler;
        private const string ViewsDir = "Views";
    }
}