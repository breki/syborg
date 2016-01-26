using System;
using System.Globalization;
using System.IO;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.Tasks.AnalysisTasks;
using Flubu.Builds.Tasks.NuGetTasks;
using Flubu.Builds.Tasks.TestingTasks;
using Flubu.Targeting;
using Flubu.Tasks.Iis;
using Flubu.Tasks.Iis.Iis7;

//css_ref Flubu.dll;
//css_ref Flubu.Contrib.dll;
//css_inc Iis7CreateAppPoolTask.cs;
//css_inc Iis7CreateWebApplicationTask.cs;

namespace BuildScripts
{
    public class BuildScript
    {
        public static int Main(string[] args)
        {
            DefaultBuildScriptRunner runner = new DefaultBuildScriptRunner(ConstructTargets, ConfigureBuildProperties);
            return runner.Run(args);
        }

        private static void ConstructTargets (TargetTree targetTree)
        {
            targetTree.AddTarget("clean.output.debug")
                .DependsOn("load.solution")
                .Do(c =>
                        {
                            c.Properties.Set(BuildProps.BuildConfiguration, "Debug");
                            targetTree.GetTarget("clean.output").Execute(c);
                        });

            targetTree.AddTarget ("rebuild")
                .SetAsDefault ()
                .SetDescription ("Builds the library and runs tests on it")
                .DependsOn ("setup.iis", "compile", "dupfinder", "tests");

            targetTree.AddTarget("release")
                .SetDescription ("Builds the library, runs tests on it and publishes it on the NuGet server")
                .DependsOn ("rebuild", "nuget");

            targetTree.GetTarget ("fetch.build.version")
                .Do (TargetFetchBuildVersion);

            targetTree.AddTarget("setup.iis")
                .SetDescription("Sets up a test web application in IIS")
                .Do(TargetSetupIis);

            targetTree.AddTarget ("dupfinder")
                .SetDescription ("Runs R# dupfinder to find code duplicates")
                .Do (TargetDupFinder);

            targetTree.AddTarget("tests")
                .SetDescription("Runs tests on the project")
                .Do (TargetRunTests).DependsOn ("load.solution");

            targetTree.AddTarget ("nuget")
                .SetDescription ("Produces NuGet packages for the library and publishes them to the NuGet server")
                .Do (c =>
                {
                    TargetNuGet (c, "Syborg");
                }).DependsOn ("prepare.build.dir", "fetch.build.version");
        }

        private static void ConfigureBuildProperties (TaskSession session)
        {
            session.Properties.Set (BuildProps.CompanyName, "Igor Brejc");
            session.Properties.Set (BuildProps.CompanyCopyright, "Copyright (C) 2014-2016 Igor Brejc");
            session.Properties.Set (BuildProps.ProductId, "Syborg");
            session.Properties.Set (BuildProps.ProductName, "Syborg");
            session.Properties.Set (BuildProps.SolutionFileName, "Syborg.sln");
            session.Properties.Set (BuildProps.TargetDotNetVersion, FlubuEnvironment.Net40VersionNumber);
            session.Properties.Set (BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);
        }

        private static void TargetFetchBuildVersion (ITaskContext context)
        {
            Version version = BuildTargets.FetchBuildVersionFromFile (context);
            context.Properties.Set (BuildProps.BuildVersion, version);
            context.WriteInfo ("The build version will be {0}", version);
        }

        private static void TargetDupFinder (ITaskContext context)
        {
            RunDupFinderAnalysisTask task = new RunDupFinderAnalysisTask ();
            task.Execute (context);
        }

        private static void TargetRunTests (ITaskContext context)
        {
            NUnitWithDotCoverTask task = new NUnitWithDotCoverTask (
                @"packages\NUnit.Console.3.0.1\tools\nunit3-console.exe",
                string.Format (CultureInfo.InvariantCulture, @"Syborg.Tests\bin\{0}\Syborg.Tests.dll", context.Properties[BuildProps.BuildConfiguration]));
            task.FailBuildOnViolations = false;
            task.NUnitCmdLineOptions = "--labels=All --verbose";
            task.DotCoverFilters = "-:module=*.Tests;-:class=*Contract;-:class=*Contract`*;-:module=LibroLib*";
            task.FailBuildOnViolations = false;
            task.Execute (context);
        }

        private static void TargetNuGet (ITaskContext context, string projectName)
        {
            string nuspecFileName = Path.Combine (projectName, projectName) + ".nuspec";

            PublishNuGetPackageTask publishTask = new PublishNuGetPackageTask (
                projectName, nuspecFileName);
            publishTask.BasePath = Path.GetFullPath (projectName);
            publishTask.ForApiKeyUseEnvironmentVariable ();
            publishTask.Execute (context);
        }

        private static void TargetSetupIis(ITaskContext context)
        {
            const string AppPoolName = "syborg";

            Iis7CreateAppPoolTask createAppPoolTask = new Iis7CreateAppPoolTask();
            createAppPoolTask.ApplicationPoolName = AppPoolName;
            createAppPoolTask.Mode = CreateApplicationPoolMode.UpdateIfExists;
            createAppPoolTask.AppPoolCustomSetupAction = pool => { pool.ManagedRuntimeVersion = "v4.0"; };
            createAppPoolTask.Execute(context);

            Iis7CreateWebApplicationTask createWebApplicationTask = new Iis7CreateWebApplicationTask();
            createWebApplicationTask.AllowAnonymous = true;
            createWebApplicationTask.ApplicationName = "syborg-tests";
            createWebApplicationTask.LocalPath = Path.GetFullPath("Syborg.WebTests");
            createWebApplicationTask.Mode = CreateWebApplicationMode.UpdateIfExists;
            createWebApplicationTask.ApplicationPoolName = AppPoolName;
            createWebApplicationTask.Execute(context);
        }
    }
}