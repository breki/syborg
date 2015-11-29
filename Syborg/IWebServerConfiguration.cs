using System;

namespace Syborg
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
    public interface IWebServerConfiguration
    {
        HttpsMode HttpsMode { get; }
        int? HttpsPort { get; }
        TimeSpan SimulatedResponseLag { get; }
        bool UseHsts { get; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
        bool WebServerDevelopmentMode { get; }

        /// <summary>
        /// Gets the root directory containing web server assets.
        /// <remarks>This property is only relevant when <see cref="WebServerDevelopmentMode"/>
        /// is switched on. It allows the changes of various files to reflect in the web application without the need for recompilation.
        /// </remarks>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebServer")]
        string WebServerDevelopmentRootDir { get; }
    }
}