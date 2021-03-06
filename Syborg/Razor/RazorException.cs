﻿using System;
using System.CodeDom.Compiler;
using System.Diagnostics.Contracts;
using System.Web.Razor;

namespace Syborg.Razor
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class RazorException : Exception
    {
        public RazorException(GeneratorResults generatorResults)
        {
            Contract.Requires(generatorResults != null);

            this.generatorResults = generatorResults;
        }

        public RazorException (GeneratorResults generatorResults, CompilerResults compilerResults)
        {
            Contract.Requires(generatorResults != null);

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