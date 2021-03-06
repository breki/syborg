﻿using System;
using System.IO;
using System.Net;
using System.Reflection;
using LibroLib.FileSystem;
using log4net;
using Syborg.Caching;
using Syborg.CommandResults;
using Syborg.ContentHandling;
using Syborg.Routing;

namespace Syborg.Commands
{
    public class FaviconCommand : IWebCommand
    {
        public FaviconCommand (
            string faviconsDirectory,
            IFileSystem fileSystem,
            IFileCache fileCache)
        {
            this.faviconsDirectory = faviconsDirectory;
            this.fileSystem = fileSystem;
            this.fileCache = fileCache;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IWebCommandResult Execute (IWebContext context, WebRequestRouteMatch routeMatch)
        {
            string path = "favicon-" + routeMatch["path"];

            // do not allow rooted (absolute) paths
            if (Path.IsPathRooted(path))
            {
                HttpStatusResult httpStatusResult = new HttpStatusResult (HttpStatusCode.BadRequest);
                httpStatusResult.LoggingSeverity = LoggingSeverity.Error;
                return httpStatusResult;                
            }

            string fileFullPath = Path.Combine (faviconsDirectory, path);

            // do not allow access outside of the contents folder (and its children)
            fileFullPath = Path.GetFullPath(fileFullPath);

            if (!fileFullPath.StartsWith (faviconsDirectory, StringComparison.Ordinal))
                return new HttpStatusResult(HttpStatusCode.Forbidden);

            if (fileSystem.DoesFileExist (fileFullPath))
            {
                ICachingPolicy cachingPolicy;

                if (context.Configuration.WebServerDevelopmentMode)
                    cachingPolicy = new NoCachingPolicy ();
                else
                    cachingPolicy = new CachingByMaxAgePolicy(TimeSpan.FromDays(30), FileLastModifiedFunc);

                FileResult fileResult = new FileResult (fileFullPath, cachingPolicy);
                fileResult.LoggingSeverity = LoggingSeverity.Verbose;
                fileResult.FileCache = fileCache;
                return fileResult;
            }

            if (log.IsDebugEnabled)
                log.DebugFormat ("Request file {0} does not exist", fileFullPath);

            HttpStatusResult notFoundResult = new HttpStatusResult(HttpStatusCode.NotFound);
            notFoundResult.LoggingSeverity = LoggingSeverity.Verbose;
            return notFoundResult;
        }

        private DateTime? FileLastModifiedFunc (object fileNameObj)
        {
            string fileName = (string)fileNameObj;
            if (!fileSystem.DoesFileExist (fileName))
                return null;

            return fileSystem.GetFileInformation (fileName).LastWriteTime;
        }

        private readonly string faviconsDirectory;
        private readonly IFileSystem fileSystem;
        private readonly IFileCache fileCache;
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);
    }
}