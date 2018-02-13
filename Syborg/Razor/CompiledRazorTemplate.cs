using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Razor;
using LibroLib;
using log4net;

namespace Syborg.Razor
{
    public class CompiledRazorTemplate : ICompiledRazorTemplate
    {
        public CompiledRazorTemplate(
            GeneratorResults generatorResults,
            CompilerResults compilerResults)
        {
            this.generatorResults = generatorResults;
            this.compilerResults = compilerResults;
        }

        public ICompiledRazorTemplate LayoutTemplate { get; set; }

        public string Execute(RazorEngineExecutionSettings executionSettings)
        {
            if (LayoutTemplate != null)
                return LayoutTemplate.ExecuteWithLayout(this, executionSettings);

            object template = CreateTemplateInstance();
            SetTemplateProperties(executionSettings, template);
            ExecuteMethod(template);
            return FetchTemplateOutput(template);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string ExecuteWithLayout (ICompiledRazorTemplate innerTemplate, RazorEngineExecutionSettings executionSettings)
        {
            if (LayoutTemplate != null)
                throw new InvalidOperationException("Cannot execute this method on an inner template");

            object innerTemplateInstance = innerTemplate.CreateTemplateInstance ();

            SetTemplateProperties (executionSettings, innerTemplateInstance);
            ExecuteMethod (innerTemplateInstance);
            string innerBody = FetchTemplateOutput (innerTemplateInstance);

            object layoutTemplateInstance = CreateTemplateInstance ();
            SetLayoutInnerTemplate(layoutTemplateInstance, innerTemplateInstance, innerBody);

            ExecuteMethod (layoutTemplateInstance);

            return FetchTemplateOutput (layoutTemplateInstance);
        }

        public object CreateTemplateInstance()
        {
            Type templateType = compilerResults.CompiledAssembly.GetExportedTypes ().Single ();
            object template = Activator.CreateInstance(templateType);
            return template;
        }

        private static void SetLayoutInnerTemplate(object layoutTemplate, object innerTemplate, string innerBody)
        {
            Type layoutTemplateType = layoutTemplate.GetType ();
            PropertyInfo innerTemplateProperty = layoutTemplateType.GetProperty ("InnerTemplate");

            if (innerTemplateProperty == null)
                throw new InvalidOperationException ("Template type {0} does not have the required string InnerTemplate property"
                    .Fmt(layoutTemplateType.FullName));

            innerTemplateProperty.SetValue (layoutTemplate, innerTemplate, null);

            PropertyInfo innerBodyProperty = layoutTemplateType.GetProperty ("InnerTemplateBody");

            if (innerBodyProperty == null)
                throw new InvalidOperationException ("Template type {0} does not have the required string InnerTemplateBody property"
                    .Fmt(layoutTemplateType.FullName));

            innerBodyProperty.SetValue (layoutTemplate, innerBody, null);

            Type innerTemplateType = innerTemplate.GetType ();
            PropertyInfo outerTemplateProperty = innerTemplateType.GetProperty ("OuterTemplate");

            if (outerTemplateProperty == null)
                throw new InvalidOperationException ("Template type {0} does not have the required string OuterTemplate property"
                    .Fmt (innerTemplateType.FullName));

            outerTemplateProperty.SetValue (innerTemplate, layoutTemplate, null);
        }

        private static void SetTemplateProperties(
            RazorEngineExecutionSettings executionSettings, object template)
        {
            foreach (KeyValuePair<string, object> propertyPair in executionSettings.Properties)
            {
                try
                {
                    Type compiledTemplateType = template.GetType();
                    PropertyInfo property = compiledTemplateType.GetProperty(propertyPair.Key);

                    if (property != null)
                    {
                        property.SetValue(template, propertyPair.Value, null);
                    }
                    else
                    {
                        //throw new InvalidOperationException("Property {0} does not exist".Fmt(propertyPair.Key));
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat(
                        "Cannot set template property '{0}' to '{1}: {2}",
                        propertyPair.Key,
                        propertyPair.Value,
                        ex);
                    throw;
                }
            }
        }

        private static void ExecuteMethod(object template)
        {
            Type templateType = template.GetType();
            MethodInfo executeMethod = templateType.GetMethod("Execute");

            if (executeMethod == null)
                throw new InvalidOperationException(
                    "Template {0} does not have the Execute() method.".Fmt(
                        templateType.FullName));

            executeMethod.Invoke(template, null);
        }

        private static string FetchTemplateOutput(object template)
        {
            Type templateType = template.GetType();
            PropertyInfo builderProperty = templateType.GetProperty("OutputBuilder");

            if (builderProperty == null)
                throw new InvalidOperationException("Template is missing the OutputBuilder property.");

            StringBuilder outputBuilder = (StringBuilder)builderProperty.GetValue(template, null);
            return outputBuilder.ToString();
        }

        private readonly GeneratorResults generatorResults;
        private readonly CompilerResults compilerResults;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}