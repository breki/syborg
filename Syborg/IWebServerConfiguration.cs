using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Syborg
{
    [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
    [ContractClass(typeof(IWebServerConfigurationContract))]
    public interface IWebServerConfiguration
    {
        HttpsMode HttpsMode { get; }
        int? HttpsPort { get; }
        TimeSpan SimulatedResponseLag { get; }
        bool UseHsts { get; }
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
        bool WebServerDevelopmentMode { get; }

        /// <summary>
        /// Gets the root directory containing web server assets.
        /// <remarks>This property is only relevant when <see cref="WebServerDevelopmentMode"/>
        /// is switched on. It allows the changes of various files to reflect in the web application without the need for recompilation.
        /// </remarks>
        /// </summary>
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
        string WebServerDevelopmentRootDir { get; }
    }

    [ContractClassFor(typeof(IWebServerConfiguration))]
    // ReSharper disable once InconsistentNaming
    internal abstract class IWebServerConfigurationContract : IWebServerConfiguration
    {
        HttpsMode IWebServerConfiguration.HttpsMode
        {
            get { throw new NotImplementedException(); }
        }

        int? IWebServerConfiguration.HttpsPort
        {
            get
            {
                Contract.Ensures (Contract.Result<int?>() != null
                    || (Contract.Result<int?>().Value >= 0 && Contract.Result<int?>().Value <= 0xffff));
                throw new NotImplementedException();
            }
        }

        bool IWebServerConfiguration.UseHsts
        {
            get { throw new NotImplementedException(); }
        }

        TimeSpan IWebServerConfiguration.SimulatedResponseLag
        {
            get { throw new NotImplementedException(); }
        }

        bool IWebServerConfiguration.WebServerDevelopmentMode
        {
            get { throw new NotImplementedException(); }
        }

        string IWebServerConfiguration.WebServerDevelopmentRootDir
        {
            get { throw new NotImplementedException(); }
        }
    }
}