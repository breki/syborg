using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using LibroLib;
using LibroLib.FileSystem;
using log4net;
using Syborg.Caching;
using Syborg.CommandResults;
using Syborg.ContentHandling;
using Syborg.Routing;

namespace Syborg.Commands
{
    public class ContentCommand : IWebCommand, IContentRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentCommand"/> class. 
        /// </summary>
        /// <param name="contentRootDirectory">
        /// The path to the root content directory.
        /// </param>
        /// <param name="fileSystem">
        /// Instance of the <see cref="IFileSystem"/> service.
        /// </param>
        /// <param name="fileCache">
        /// Instance of the <see cref="IFileCache"/> service. If <c>null</c>, no file caching will be performed.
        /// </param>
        public ContentCommand (
            string contentRootDirectory,
            IFileSystem fileSystem,
            IFileCache fileCache)
        {
            Contract.Requires (contentRootDirectory != null);
            Contract.Requires(fileSystem != null);

            this.contentRootDirectory = contentRootDirectory;
            this.fileSystem = fileSystem;
            this.fileCache = fileCache;
        }

        /// <summary>
        /// Gets or sets a value indicating whether gzip compression is supported for files returned by this <see cref="ContentCommand"/>.
        /// </summary>
        /// <remarks>gzip is enabled by default.</remarks>
        public bool AllowGzipCompression
        {
            get { return allowGzipCompression; }
            set { allowGzipCompression = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IWebCommandResult Execute (IWebContext context, WebRequestRouteMatch routeMatch)
        {
            // do not allow rooted (absolute) paths
            string path = routeMatch["path"];

            if (path == null)
                return new HttpStatusResult (HttpStatusCode.BadRequest);

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
                result.AllowGzipCompression = allowGzipCompression;
                result.FileCache = fileCache;
                result.LoggingSeverity = LoggingSeverity.Verbose;
                return result;
            }

            if (log.IsDebugEnabled)
                log.DebugFormat ("Requested file '{0}' does not exist", fileFullPath);

            HttpStatusResult notFoundResult = new HttpStatusResult(HttpStatusCode.NotFound);
            notFoundResult.LoggingSeverity = LoggingSeverity.Error;
            return notFoundResult;
        }

        public ContentCommand CacheByMaxAge (string pattern, TimeSpan maxAge)
        {
            Contract.Requires(pattern != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<ContentCommand>(), this));

            cachingRules.Add(CachingRule.ByMaxAge(pattern, maxAge, FileLastModifiedFunc));
            return this;
        }

        public ContentCommand CacheByETag (string pattern, TimeSpan maxAge, Func<Tuple<string, DateTime?>> etagFunc)
        {
            Contract.Requires(pattern != null);
            Contract.Requires(etagFunc != null);
            Contract.Ensures(ReferenceEquals(Contract.Result<ContentCommand>(), this));

            cachingRules.Add (CachingRule.ByETag (pattern, maxAge, etagFunc));
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
        private readonly IFileCache fileCache;
        private readonly List<CachingRule> cachingRules = new List<CachingRule>();
        private bool allowGzipCompression = true;
        private static readonly ILog log = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);

        private class CachingRule
        {
            public static CachingRule ByMaxAge (string pattern, TimeSpan maxAge, Func<object, DateTime?> fileLastModifiedFunc)
            {
                Contract.Requires(pattern != null);
                Contract.Ensures(Contract.Result<CachingRule>() != null);

                return new CachingRule(
                    CreatePatternRegex(pattern), 
                    new CachingByMaxAgePolicy (maxAge, fileLastModifiedFunc));
            }

            public static CachingRule ByETag (string pattern, TimeSpan maxAge, Func<Tuple<string, DateTime?>> etagFunc)
            {
                Contract.Requires(pattern != null);
                Contract.Ensures(Contract.Result<CachingRule>() != null);

                return new CachingRule (
                    CreatePatternRegex (pattern),
                    new CachingByETagPolicy(maxAge, etagFunc));
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

            private CachingRule(Regex patternRegex, ICachingPolicy cachingPolicy)
            {
                Contract.Requires (patternRegex != null);
                Contract.Requires (cachingPolicy != null);

                this.patternRegex = patternRegex;
                this.cachingPolicy = cachingPolicy;
            }

            private static Regex CreatePatternRegex(string pattern)
            {
                return new Regex(pattern.WildcardsToRegex(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            private readonly Regex patternRegex;
            private readonly ICachingPolicy cachingPolicy;
        }
    }
}
