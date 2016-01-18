using NUnit.Framework;

namespace Syborg.Tests
{
    [SetUpFixture]
    public class AssemblyFixture
    {
        [OneTimeSetUp]
        public void Setup ()
        {
            log4net.Config.XmlConfigurator.Configure ();
        }

        [OneTimeTearDown]
        public void Teardown ()
        {
            log4net.LogManager.Shutdown ();
        }
    }
}
