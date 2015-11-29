using NUnit.Framework;

namespace Syborg.Tests
{
    [SetUpFixture]
    public class AssemblyFixture
    {
        [SetUp]
        public void Setup ()
        {
            log4net.Config.XmlConfigurator.Configure ();
        }

        [TearDown]
        public void Teardown ()
        {
            log4net.LogManager.Shutdown ();
        }
    }
}
