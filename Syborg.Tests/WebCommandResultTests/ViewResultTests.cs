using LibroLib.Misc;
using NUnit.Framework;
using Rhino.Mocks;
using Syborg.CommandResults;
using Syborg.Common;
using Syborg.Razor;

namespace Syborg.Tests.WebCommandResultTests
{
    public class ViewResultTests
    {
        [Test]
        public void RenderViewByModelType()
        {
            ICompiledRazorTemplate template = MockRepository.GenerateMock<ICompiledRazorTemplate>();
            template.Expect(x => x.Execute(null)).IgnoreArguments().Return("rendered view");

            viewRenderingEngine.Expect(x => x.GetViewTemplateByModel<SampleViewModel>()).Return(template);

            SampleViewModel model = new SampleViewModel();
            ViewResult<SampleViewModel> result = new ViewResult<SampleViewModel>(model, razorEngineExecutionSettings);
            result.Apply(context);

            viewRenderingEngine.VerifyAllExpectations();
            template.VerifyAllExpectations();
        }

        [Test]
        public void RenderViewByName()
        {
            ICompiledRazorTemplate template = MockRepository.GenerateMock<ICompiledRazorTemplate>();
            template.Expect(x => x.Execute(null)).IgnoreArguments().Return("rendered view");

            viewRenderingEngine.Expect(x => x.GetViewTemplateByName("SomeModel")).Return(template);

            SampleViewModel model = new SampleViewModel();
            ViewResult<SampleViewModel> result = new ViewResult<SampleViewModel>("SomeModel", model, razorEngineExecutionSettings);
            result.Apply(context);

            viewRenderingEngine.VerifyAllExpectations();
            template.VerifyAllExpectations();
        }

        [SetUp]
        public void Setup ()
        {
            //fileSystem = MockRepository.GenerateStub<IFileSystem> ();
            ITimeService timeService = MockRepository.GenerateStub<ITimeService>();
            //IWebServerConfiguration configuration = MockRepository.GenerateStub<IWebServerConfiguration> ();
            //configuration.Stub (x => x.WebServerDevelopmentMode).Return (false);
            //context = new FakeWebContext (null, null, fileSystem, timeService, configuration);

            context = new FakeWebContext (null, null, null, timeService, null);
            viewRenderingEngine = MockRepository.GenerateMock<IRazorViewRenderingEngine>();
            context.ViewRenderingEngine = viewRenderingEngine;

            razorEngineExecutionSettings = new RazorEngineExecutionSettings ();
        }

        private FakeWebContext context;
        //private IFileSystem fileSystem;
        private IRazorViewRenderingEngine viewRenderingEngine;
        private RazorEngineExecutionSettings razorEngineExecutionSettings;
    }
}