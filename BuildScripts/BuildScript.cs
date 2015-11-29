using System;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Targeting;
using Flubu.Tasks.Processes;

//css_ref Flubu.dll;
//css_ref Flubu.Contrib.dll;

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
                .DependsOn ("compile", "tests");

            targetTree.AddTarget("release")
                .SetDescription ("Builds the library, runs tests on it and publishes it on the NuGet server")
                .DependsOn ("rebuild");

            targetTree.GetTarget ("fetch.build.version")
                .Do (TargetFetchBuildVersion);

            targetTree.AddTarget("tests")
                .SetDescription("Runs tests on the project")
                .Do (r =>
                    {
                        TargetRunTests(r, "Syborg.Tests", ".dll");
                    }).DependsOn ("load.solution");
        }

        private static void ConfigureBuildProperties (TaskSession session)
        {
            session.Properties.Set (BuildProps.CompanyName, "Igor Brejc");
            session.Properties.Set (BuildProps.CompanyCopyright, "Copyright (C) 2014-2015 Igor Brejc");
            session.Properties.Set (BuildProps.ProductId, "Syborg");
            session.Properties.Set (BuildProps.ProductName, "Syborg");
            session.Properties.Set (BuildProps.SolutionFileName, "Syborg.sln");
            session.Properties.Set (BuildProps.TargetDotNetVersion, FlubuEnvironment.Net40VersionNumber);
            session.Properties.Set (BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);
        }

        private static void TargetFetchBuildVersion (ITaskContext context)
        {
            Version version = BuildTargets.FetchBuildVersionFromFile (context);
            version = new Version (version.Major, version.Minor, BuildTargets.FetchBuildNumberFromFile (context));
            context.Properties.Set (BuildProps.BuildVersion, version);
            context.WriteInfo ("The build version will be {0}", version);
        }

        private static void TargetRunTests(ITaskContext context, string projectName, string extension)
        {
            VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);
            string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);

            VSProjectWithFileInfo project =
                (VSProjectWithFileInfo)solution.FindProjectByName(projectName);
            FileFullPath projectTarget = project.ProjectDirectoryPath.CombineWith(project.GetProjectOutputPath(buildConfiguration))
                .AddFileName("{0}{1}", project.ProjectName, extension);

            RunProgramTask task = new RunProgramTask(
                @"packages\NUnit.Runners.2.6.3\tools\nunit-console.exe")
                .AddArgument(projectTarget.ToString())
                .AddArgument("/framework:net-4.0")
                .AddArgument("/labels")
                .AddArgument("/trace=Verbose")
                .AddArgument("/nodots");
            task.Execute(context);
        }
    }
}