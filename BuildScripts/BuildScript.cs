using System;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Targeting;
using Flubu.Tasks.Processes;

//css_ref Flubu.dll;
//css_ref Flubu.Contrib.dll;
//css_ref log4net.dll;
//css_ref ICSharpCode.SharpZipLib.dll;
//css_ref Brejc.Common.Library.dll;

namespace BuildScripts
{
    public class BuildScript
    {
        public static int Main(string[] args)
        {
            TargetTree targetTree = new TargetTree();
            BuildTargets.FillBuildTargets(targetTree);

            targetTree.AddTarget("clean.output.debug")
                .DependsOn("load.solution")
                .Do(c =>
                        {
                            c.Properties.Set(BuildProps.BuildConfiguration, "Debug");
                            targetTree.GetTarget("clean.output").Execute(c);
                        });

            targetTree.AddTarget ("set.full.config").SetAsHidden ()
                .Do (c => c.Properties.Set (BuildProps.BuildConfiguration, "Full"));

            targetTree.AddTarget ("rebuild")
                .SetAsDefault ()
                .SetDescription ("Builds the product, packages it, deploys and tests it on a local instance")
                .DependsOn ("compile", "tests", "package", "system.tests");

            targetTree.AddTarget ("rebuild.local.full")
                .SetDescription ("Builds the product in the Full configuration (code analysis, code contracts), tests it and prepares the installation packages (local developer build)")
                .DependsOn ("set.full.config", "rebuild");

            targetTree.AddTarget ("set.full-syborg.config").SetAsHidden ()
                .Do (c => c.Properties.Set (BuildProps.BuildConfiguration, "Full-Syborg"));
            targetTree.AddTarget ("rebuild.syborg")
                .SetDescription ("Builds Syborg, packages it, deploys and tests it on a local instance")
                .DependsOn ("set.full-syborg.config", "compile", "tests.syborg", "package.syborg");

            targetTree.AddTarget("release")
                .SetDescription("Builds the product, packages it, deploys and tests it on a local instance and then uploads it to the production server")
                //.DependsOn ("compile", "package", "upload.package");
                .DependsOn ("rebuild", "upload.package");

            targetTree.AddTarget("quick.deploy")
                .SetDescription("Builds the product, packages and uploads it to production server (without tests!)")
                .DependsOn ("compile", "package", "upload.package");

            targetTree.GetTarget ("fetch.build.version")
                .Do (TargetFetchBuildVersion);

            targetTree.AddTarget("tests")
                .SetDescription("Runs tests on the project")
                .Do (r =>
                    {
                        TargetRunTests(r, "Syborg.Tests", ".dll");
                    }).DependsOn ("load.solution");

            using (TaskSession session = new TaskSession(new SimpleTaskContextProperties(), args, targetTree))
            {
                BuildTargets.FillDefaultProperties(session);
                session.Start(BuildTargets.OnBuildFinished);

                session.AddLogger(new MulticoloredConsoleLogger(Console.Out));

                session.Properties.Set(BuildProps.CompanyName, "Igor Brejc");
                session.Properties.Set(BuildProps.CompanyCopyright, "Copyright (C) 2014-2015 Igor Brejc");
                session.Properties.Set(BuildProps.ProductId, "Syborg");
                session.Properties.Set (BuildProps.ProductName, "Syborg");
                session.Properties.Set (BuildProps.SolutionFileName, "Syborg.sln");
                session.Properties.Set(BuildProps.TargetDotNetVersion, FlubuEnvironment.Net40VersionNumber);
                session.Properties.Set(BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);

                try
                {
                    // actual run
                    if (args.Length == 0)
                        targetTree.RunTarget(session, targetTree.DefaultTarget.TargetName);
                    else
                    {
                        string targetName = args[0];
                        if (false == targetTree.HasTarget(targetName))
                        {
                            session.WriteError("ERROR: The target '{0}' does not exist", targetName);
                            targetTree.RunTarget(session, "help");
                            return 2;
                        }

                        targetTree.RunTarget(session, args[0]);
                    }

                    session.Complete();

                    return 0;
                }
                catch (TaskExecutionException)
                {
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return 1;
                }
            }
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