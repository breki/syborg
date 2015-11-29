using System;
using System.CodeDom.Compiler;
using System.Web.Razor;

namespace Syborg.Razor
{
    public class RazorException : Exception
    {
        public RazorException(GeneratorResults generatorResults)
        {
            this.generatorResults = generatorResults;
        }

        public RazorException (GeneratorResults generatorResults, CompilerResults compilerResults)
        {
            this.generatorResults = generatorResults;
            this.compilerResults = compilerResults;
        }

        public override string Message
        {
            get
            {
                if (!generatorResults.Success)
                    return "Template parse errors";

                return "Compile errors";
            }
        }

        public GeneratorResults GeneratorResults
        {
            get { return generatorResults; }
        }

        public CompilerResults CompilerResults
        {
            get { return compilerResults; }
        }

        private readonly GeneratorResults generatorResults;
        private readonly CompilerResults compilerResults;
    }
}