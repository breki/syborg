using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Web.Razor.Parser.SyntaxTree;
using log4net;
using Syborg.Common;

namespace Syborg.Razor
{
    public class RazorViewRenderingEngine : IRazorViewRenderingEngine
    {
        public RazorViewRenderingEngine(
            IFileSystem fileSystem, 
            IRazorCompiler razorCompiler)
        {
            Contract.Requires(fileSystem != null);
            Contract.Requires(razorCompiler != null);

            this.fileSystem = fileSystem;
            this.razorCompiler = razorCompiler;
        }

        public void Initialize (string viewsRootDirectory, RazorEngineCompileSettings settings)
        {
            this.viewsRootDirectory = viewsRootDirectory;
            compileSettings = settings;
        }

        public void RegisterLayout (string layoutName)
        {
            string fileName;
            string viewContents = FetchViewContents (viewsRootDirectory, layoutName, out fileName);

            if (log.IsDebugEnabled)
                log.DebugFormat("Registering Razor layout '{0}'", layoutName);

            try
            {
                ICompiledRazorTemplate template = razorCompiler.Compile(viewContents, compileSettings, layoutName);
                layoutTemplates.Add(layoutName, template);
            }
            catch (RazorException ex)
            {
                LogTemplateErrors(ex, fileName);
                throw;
            }
        }

        public void RegisterView<TModel>(string layoutName = null, string nspace = null)
        {
            ICompiledRazorTemplate layoutTemplate = null;
            
            if (layoutName != null)
                if (!layoutTemplates.TryGetValue(layoutName, out layoutTemplate))
                    throw new ArgumentException("Layout '{0}' not registered".Fmt(layoutName), "layoutName");

            string viewName = ExtractViewName<TModel>();

            RegisterViewPrivate(viewName, layoutTemplate, nspace);
        }

        public void RegisterView(string viewName, string layoutName = null, string nspace = null)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat ("Registering Razor view '{0}'", viewName);

            ICompiledRazorTemplate layoutTemplate = null;
            
            if (layoutName != null)
                if (!layoutTemplates.TryGetValue(layoutName, out layoutTemplate))
                    throw new ArgumentException("Layout '{0}' not registered".Fmt(layoutName), "layoutName");

            RegisterViewPrivate (viewName, layoutTemplate, nspace);
        }

        public ICompiledRazorTemplate GetViewTemplateByModel<TModel>()
        {
            string viewName = ExtractViewName<TModel>();
            return GetViewTemplateByName(viewName);
        }

        public ICompiledRazorTemplate GetViewTemplateByName(string viewName)
        {
            ICompiledRazorTemplate template;
            if (!viewTemplates.TryGetValue (viewName, out template))
                throw new KeyNotFoundException ("Razor template for view '{0}' has not been registered".Fmt (viewName));

            return template;
        }

        private static string ExtractViewName<TModel>()
        {
            string modelTypeName = typeof(TModel).Name;
            int suffixStart = modelTypeName.IndexOf("ViewModel", StringComparison.Ordinal);

            if (suffixStart == -1)
                throw new InvalidOperationException ("The view model type '{0}' does not follow the naming convention".Fmt (typeof(TModel).FullName));

            string viewName = modelTypeName.Substring(0, suffixStart);
            return viewName;
        }

        private string FetchViewContents(string viewsDirectory, string viewName, out string viewFileName)
        {
            viewFileName = Path.Combine(viewsDirectory, viewName + ".cshtml");
            if (!fileSystem.DoesFileExist(viewFileName))
                throw new InvalidOperationException("The view file '{0}' does not exist".Fmt(viewFileName));

            string viewContents = fileSystem.ReadFileAsString(viewFileName);
            return viewContents;
        }

        private void RegisterViewPrivate (string viewName, ICompiledRazorTemplate layoutTemplate, string nspace)
        {
            if (log.IsDebugEnabled)
                log.DebugFormat("Registering Razor view '{0}'", viewName);

            RazorEngineCompileSettings viewCompileSettings = compileSettings.Clone();
            viewCompileSettings.DefaultClassName = viewName + compileSettings.DefaultClassName;

            string fileName;
            
            string containerDir;

            if (nspace == null)
                containerDir = viewsRootDirectory;
            else
                containerDir = Path.Combine(viewsRootDirectory, nspace);

            Contract.Assume(containerDir != null);
            string viewContents = FetchViewContents(containerDir, viewName, out fileName);

            try
            {
                ICompiledRazorTemplate template = razorCompiler.Compile(viewContents, viewCompileSettings, viewName);
                template.LayoutTemplate = layoutTemplate;
                viewTemplates.Add(viewName, template);
            }
            catch (RazorException ex)
            {
                LogTemplateErrors(ex, fileName);
                throw;
            }
        }

        private static void LogTemplateErrors(RazorException ex, string fileName)
        {
            if (!ex.GeneratorResults.Success)
            {
                log.ErrorFormat("Template parse errors in '{0}' Razor file:", fileName);
                foreach (RazorError error in ex.GeneratorResults.ParserErrors)
                    log.Error(error);
            }
            else if (ex.CompilerResults.Errors.HasErrors)
            {
                log.ErrorFormat("Compile errors in '{0}' Razor file:", fileName);
                foreach (CompilerError error in ex.CompilerResults.Errors)
                    log.Error(error);
            }
        }

        private string viewsRootDirectory;
        private readonly IFileSystem fileSystem;
        private readonly IRazorCompiler razorCompiler;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private RazorEngineCompileSettings compileSettings;
        private readonly Dictionary<string, ICompiledRazorTemplate> layoutTemplates = new Dictionary<string, ICompiledRazorTemplate>();
        private readonly Dictionary<string, ICompiledRazorTemplate> viewTemplates = new Dictionary<string, ICompiledRazorTemplate>();
    }
}