using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Flubu;
using Flubu.Tasks.Iis;
using Flubu.Tasks.Iis.Iis7;
using Microsoft.Web.Administration;

namespace BuildScripts
{
    public class Iis7CreateWebApplicationTask : Iis7TaskBase, ICreateWebApplicationTask
    {
        public CreateWebApplicationMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public string ApplicationHostConfigurationPath
        {
            get { return applicationHostConfigurationPath; }
            set { applicationHostConfigurationPath = value; }
        }

        public string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        public string LocalPath
        {
            get { return localPath; }
            set { localPath = Path.GetFullPath (value); }
        }

        public bool AllowAnonymous
        {
            get { return allowAnonymous; }
            set { allowAnonymous = value; }
        }

        public bool AllowAuthNtlm
        {
            get { return allowAuthNtlm; }
            set { allowAuthNtlm = value; }
        }

        public string AnonymousUserName
        {
            get { return anonymousUserName; }
            set { anonymousUserName = value; }
        }

        public string AnonymousUserPass
        {
            get { return anonymousUserPass; }
            set { anonymousUserPass = value; }
        }

        public string AppFriendlyName
        {
            get { return appFriendlyName; }
            set { appFriendlyName = value; }
        }

        public bool AspEnableParentPaths
        {
            get { return aspEnableParentPaths; }
            set { aspEnableParentPaths = value; }
        }

        public bool AccessScript
        {
            get { return accessScript; }
            set { accessScript = value; }
        }

        public bool AccessExecute { get; set; }

        public string DefaultDoc
        {
            get { return defaultDoc; }
            set { defaultDoc = value; }
        }

        public bool EnableDefaultDoc
        {
            get { return enableDefaultDoc; }
            set { enableDefaultDoc = value; }
        }

        /// <summary>
        /// Gets or sets the Name of the website that the web application is added too. By default it is "Default Web Site"
        /// </summary>
        public string WebSiteName
        {
            get { return webSiteName; }
            set { webSiteName = value; }
        }

        public string ParentVirtualDirectoryName
        {
            get { return parentVirtualDirectoryName; }
            set { parentVirtualDirectoryName = value; }
        }

        public string ApplicationPoolName
        {
            get { return applicationPoolName; }
            set { applicationPoolName = value; }
        }

        public IList<MimeType> MimeTypes { get; set; }

        public Action<Application> WebAppCustomSetupAction
        {
            get { return webAppCustomSetupAction; }
            set { webAppCustomSetupAction = value; }
        }

        public override string Description
        {
            get
            {
                return String.Format (
                    CultureInfo.InvariantCulture,
                    "Create IIS Web application '{0}' on local path '{1}'",
                    applicationName,
                    localPath);
            }
        }

        protected override void DoExecute (ITaskContext context)
        {
            if (string.IsNullOrEmpty (ApplicationName))
                throw new TaskExecutionException ("ApplicationName missing!");

            using (ServerManager serverManager = new ServerManager(applicationHostConfigurationPath))
            {
                Site site = serverManager.Sites.FirstOrDefault(x => x.Name == webSiteName);
                if (site == null)
                    throw new InvalidOperationException(string.Format("Web site '{0}' does not exist.", WebSiteName));

                string vdirPath = "/" + ApplicationName;

                Application foundApp = null;
                foreach (Application candidateApp in site.Applications)
                {
                    if (candidateApp.Path != vdirPath)
                        continue;

                    switch (mode)
                    {
                        case CreateWebApplicationMode.DoNothingIfExists:
                            context.WriteInfo("Web application '{0}' already exists, doing nothing.", applicationName);
                            return;
                        case CreateWebApplicationMode.FailIfAlreadyExists:
                            throw new TaskExecutionException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Web application '{0}' already exists.",
                                    applicationName));
                        default:
                            foundApp = candidateApp;
                            break;
                    }
                }

                if (foundApp == null)
                {
                    foundApp = site.Applications.Add(vdirPath, this.LocalPath);
                    foundApp.ApplicationPoolName = applicationPoolName;
                }

                Configuration config = foundApp.GetWebConfiguration();
                foundApp.ApplicationPoolName = applicationPoolName;

                if (webAppCustomSetupAction != null)
                    webAppCustomSetupAction(foundApp);

                AddMimeTypes(config, MimeTypes);
                serverManager.CommitChanges ();
            }
        }

        private CreateWebApplicationMode mode = CreateWebApplicationMode.FailIfAlreadyExists;
        private string applicationHostConfigurationPath;
        private string applicationName;
        private string parentVirtualDirectoryName = @"IIS://localhost/W3SVC/1/Root";
        private string localPath;
        private bool allowAnonymous = true;
        private bool allowAuthNtlm = true;
        private bool accessScript = true;
        private string anonymousUserName;
        private string anonymousUserPass;
        private string appFriendlyName;
        private bool aspEnableParentPaths;
        private string defaultDoc;
        private bool enableDefaultDoc = true;
        private string applicationPoolName = "DefaultAppPool";
        private string webSiteName = "Default Web Site";
        private Action<Application> webAppCustomSetupAction;
    }
}