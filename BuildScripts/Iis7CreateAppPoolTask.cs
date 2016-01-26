using System;
using Flubu;
using Flubu.Tasks.Iis;
using Microsoft.Web.Administration;

namespace BuildScripts
{
    public class Iis7CreateAppPoolTask : TaskBase, ICreateAppPoolTask
    {
        public string ApplicationHostConfigurationPath
        {
            get { return applicationHostConfigurationPath; }
            set { applicationHostConfigurationPath = value; }
        }

        public string ApplicationPoolName
        {
            get { return applicationPoolName; }
            set { applicationPoolName = value; }
        }

        public bool ClassicManagedPipelineMode { get; set; }

        public bool Enable32BitAppOnWin64
        {
            get { return enable32BitAppOnWin64; }
            set { enable32BitAppOnWin64 = value; }
        }

        public CreateApplicationPoolMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public Action<ApplicationPool> AppPoolCustomSetupAction
        {
            get { return appPoolCustomSetupAction; }
            set { appPoolCustomSetupAction = value; }
        }

        public override string Description
        {
            get
            {
                return string.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Create application pool '{0}'.",
                    applicationPoolName);
            }
        }

        protected override void DoExecute (ITaskContext context)
        {
            using (ServerManager serverManager = new ServerManager (applicationHostConfigurationPath))
            {
                ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;

                ApplicationPool appPoolToWorkOn = null;
                bool updatedExisting = false;

                foreach (ApplicationPool applicationPool in applicationPoolCollection)
                {
                    if (applicationPool.Name == applicationPoolName)
                    {
                        if (mode == CreateApplicationPoolMode.DoNothingIfExists)
                        {
                            context.WriteInfo (
                                "Application pool '{0}' already exists, doing nothing.",
                                applicationPoolName);
                        }
                        else if (mode == CreateApplicationPoolMode.FailIfAlreadyExists)
                        {
                            throw new TaskExecutionException (
                                String.Format (
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "Application '{0}' already exists.",
                                    applicationPoolName));
                        }

                        // otherwise we should update the existing application pool
                        appPoolToWorkOn = applicationPool;
                        updatedExisting = true;
                        break;
                    }
                }

                if (appPoolToWorkOn == null)
                    appPoolToWorkOn = serverManager.ApplicationPools.Add (applicationPoolName);

                appPoolToWorkOn.AutoStart = true;
                appPoolToWorkOn.Enable32BitAppOnWin64 = enable32BitAppOnWin64;
                appPoolToWorkOn.ManagedPipelineMode =
                    ClassicManagedPipelineMode ? ManagedPipelineMode.Classic : ManagedPipelineMode.Integrated;

                if (appPoolCustomSetupAction != null)
                    appPoolCustomSetupAction(appPoolToWorkOn);

                serverManager.CommitChanges ();

                context.WriteInfo (
                    "Application pool '{0}' {1}.",
                    applicationPoolName,
                    updatedExisting ? "updated" : "created");
            }
        }

        private string applicationHostConfigurationPath;
        private string applicationPoolName;
        private CreateApplicationPoolMode mode;
        private bool enable32BitAppOnWin64;
        private Action<ApplicationPool> appPoolCustomSetupAction;
    }
}