using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.Tasks.AnalysisTasks;
using Flubu.Builds.Tasks.NuGetTasks;
using Flubu.Builds.Tasks.TestingTasks;
using Flubu.Targeting;
using Flubu.Tasks.Processes;

namespace BuildScripts
{
    public class BuildScript : DefaultBuildScript
    {
        protected override void ConfigureTargets(TargetTree targetTree, ICollection<string> args)
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
                .DependsOn ("compile", "dupfinder", "tests");

            targetTree.AddTarget("release")
                .SetDescription ("Builds the library, runs tests on it and publishes it on the NuGet server")
                .DependsOn ("setup.iis", "rebuild", "nuget");

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

        protected override void ConfigureBuildProperties (TaskSession session)
        {
            session.Properties.Set (BuildProps.CompanyName, "Igor Brejc");
            session.Properties.Set (BuildProps.CompanyCopyright, "Copyright (C) 2014-2019 Igor Brejc");
            session.Properties.Set (BuildProps.ProductId, "Syborg");
            session.Properties.Set (BuildProps.ProductName, "Syborg");
            session.Properties.Set (BuildProps.SolutionFileName, "Syborg.sln");
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
                @"packages\NUnit.ConsoleRunner.3.10.0\tools\nunit3-console.exe",
                string.Format (CultureInfo.InvariantCulture, @"Syborg.Tests\bin\{0}\Syborg.Tests.dll", context.Properties[BuildProps.BuildConfiguration]));
            task.FailBuildOnViolations = false;
            task.NUnitCmdLineOptions = "--labels=All --trace=Verbose";
            task.DotCoverFilters = "-:module=*.Tests;-:class=*Contract;-:class=*Contract`*;-:module=LibroLib*;-:class=JetBrains.Annotations.*";

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
            RunProgramTask task = new RunProgramTask("PowerShell.exe");
            task
                .AddArgument("-executionpolicy").AddArgument("remotesigned")
                .AddArgument(@"AppVeyor\BuildInstall.ps1");
            task.Execute(context);
        }
    }
}