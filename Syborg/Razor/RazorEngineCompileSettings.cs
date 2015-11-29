using System.Collections.Generic;
using System.Reflection;

namespace Syborg.Razor
{
    public class RazorEngineCompileSettings
    {
        public string DefaultNamespace
        {
            get { return defaultNamespace; }
            set { defaultNamespace = value; }
        }

        public string DefaultClassName
        {
            get { return defaultClassName; }
            set { defaultClassName = value; }
        }

        public string DefaultBaseClass
        {
            get { return defaultBaseClass; }
            set { defaultBaseClass = value; }
        }

        public IList<string> NamespaceImports
        {
            get { return namespaceImports; }
        }

        public IList<Assembly> ReferenceAssemblies
        {
            get { return referenceAssemblies; }
        }

        public bool DebugMode
        {
            get; set;
        }

        public RazorEngineCompileSettings Clone()
        {
            RazorEngineCompileSettings clone = new RazorEngineCompileSettings();
            clone.defaultNamespace = defaultNamespace;
            clone.defaultClassName = defaultClassName;
            clone.defaultBaseClass = defaultBaseClass;
            clone.namespaceImports.AddRange(namespaceImports);
            clone.referenceAssemblies.AddRange(referenceAssemblies);
            clone.DebugMode = DebugMode;
            return clone;
        }

        private string defaultNamespace;
        private string defaultClassName;
        private string defaultBaseClass;
        private readonly List<string> namespaceImports = new List<string>();
        private readonly List<Assembly> referenceAssemblies = new List<Assembly>();
    }
}