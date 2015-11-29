using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Syborg.Caching;
using Syborg.CommandResults;
using Syborg.Common;
using Syborg.Routing;

namespace Syborg.Commands
{
    public class ContentCommand : IWebCommand, IContentRepository
    {
        public ContentCommand (
            string contentRootDirectory,
            IFileSystem fileSystem)
        {
            if (contentRootDirectory == null)
                throw new ArgumentNullException ("contentRootDirectory");

            this.contentRootDirectory = contentRootDirectory;
            this.fileSystem = fileSystem;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IWebCommandResult Execute (IWebContext context, WebRequestRouteMatch routeMatch)
        {
            // do not allow rooted (absolute) paths
            string path = routeMatch["path"];
            if (Path.IsPathRooted(path))
            {
                HttpStatusResult httpStatusResult = new HttpStatusResult (HttpStatusCode.BadRequest);
                httpStatusResult.LoggingSeverity = LoggingSeverity.Error;
                return httpStatusResult;                
            }

            string fileFullPath = Path.Combine (contentRootDirectory, path);

            // do not allow access outside of the contents folder (and its children)
            fileFullPath = Path.GetFullPath(fileFullPath);

            if (!fileFullPath.StartsWith(contentRootDirectory, StringComparison.Ordinal))
                return new HttpStatusResult(HttpStatusCode.Forbidden);

            if (fileSystem.DoesFileExist(fileFullPath))
            {
                ICachingPolicy cachingPolicy;

                if (context.Configuration.WebServerDevelopmentMode)
                    cachingPolicy = new NoCachingPolicy();
                else
                {
                    // choose the policy based on wildcard match
                    cachingPolicy = FindCachingPolicyForFile (path);
                }

                if (log.IsDebugEnabled)
                    log.DebugFormat("Returning contents of the file '{0}'", fileFullPath);

                FileResult result = new FileResult(fileFullPath, cachingPolicy);
                result.LoggingSeverity = LoggingSeverity.Verbose;
                return result;
            }

            if (log.IsDebugEnabled)
                log.DebugFormat ("Requested file '{0}' does not exist", fileFullPath);

            HttpStatusResult notFoundResult = new HttpStatusResult(HttpStatusCode.NotFound);
            notFoundResult.LoggingSeverity = LoggingSeverity.Error;
            return notFoundResult;
        }

        public ContentCommand CacheWithMaxAge (string pattern, TimeSpan maxAge)
        {
            Contract.Requires(pattern != null);
            cachingRules.Add(new CachingRule(pattern, maxAge, FileLastModifiedFunc));
            return this;
        }

        public bool DoesFileExist (string contentFilePath)
        {
            string fileFullPath = Path.Combine (contentRootDirectory, contentFilePath);

            // do not allow access outside of the contents folder (and its children)
            fileFullPath = Path.GetFullPath(fileFullPath);

            if (!fileFullPath.StartsWith(contentRootDirectory, StringComparison.Ordinal))
                throw new InvalidOperationException("Access outside of content directory not allowed");

            return fileSystem.DoesFileExist(fileFullPath);
        }

        private DateTime? FileLastModifiedFunc(object fileNameObj)
        {
            Contract.Requires(fileNameObj != null);
            string fileName = (string)fileNameObj;
            if (!fileSystem.DoesFileExist(fileName))
                return null;

            return fileSystem.GetFileInformation(fileName).LastWriteTime;
        }

        private ICachingPolicy FindCachingPolicyForFile(string path)
        {
            Contract.Ensures(Contract.Result<ICachingPolicy>() != null);
            //if (log.IsDebugEnabled)
            //    log.DebugFormat ("FindCachingPolicyForFile ('{0}')", path);

            foreach (CachingRule rule in cachingRules)
            {
                if (rule.IsMatchFor(path))
                    return rule.CachingPolicy;
            }

            return new NoCachingPolicy();
        }

        private readonly string contentRootDirectory;
        private readonly IFileSystem fileSystem;
        private readonly List<CachingRule> cachingRules = new List<CachingRule>();
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);

        private class CachingRule
        {
            public CachingRule (string pattern, TimeSpan maxAge, Func<object, DateTime?> fileLastModifiedFunc)
            {
                Contract.Requires(pattern != null);
                patternRegex = new Regex(pattern.WildcardsToRegex(), RegexOptions.IgnoreCase | RegexOptions.Compiled);

                cachingPolicy = new CachingByMaxAgePolicy (maxAge, fileLastModifiedFunc);
            }

            public ICachingPolicy CachingPolicy
            {
                get { return cachingPolicy; }
            }

            public bool IsMatchFor (string path)
            {
                bool isMatch = patternRegex.IsMatch (path);

                //if (log.IsDebugEnabled)
                //    log.DebugFormat("{0} : {1}", patternRegex, isMatch);

                return isMatch;
            }

            private readonly Regex patternRegex;
            private readonly ICachingPolicy cachingPolicy;
        }
    }
}
