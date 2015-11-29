using System.Collections.Generic;

namespace Syborg.Razor
{
    public class RazorEngineExecutionSettings
    {
        public IDictionary<string, object> Properties
        {
            get { return properties; }
        }

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();
    }
}