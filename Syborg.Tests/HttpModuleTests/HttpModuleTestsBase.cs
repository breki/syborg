using LibroLib.WebUtils;
using LibroLib.WebUtils.Rest;
using NUnit.Framework;

namespace Syborg.Tests.HttpModuleTests
{
    public class HttpModuleTestsBase
    {
        public const string WebAppUrl = "http://localhost/syborg-tests/";

        [SetUp]
        public virtual void Setup ()
        {
            IWebConfiguration webConfiguration = new WebConfiguration ("Syborg.Tests");
            restClientFactory = new RestClientFactory (webConfiguration);
        }

        protected RestClientFactory RestClientFactory
        {
            get { return restClientFactory; }
        }

        private RestClientFactory restClientFactory;
    }
}