using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using LibroLib.Text;
using log4net;

namespace Syborg.ContentHandling
{
    public class FileCache : IFileCache
    {
        public long CachedFilesTotalSize
        {
            get { return cache.Sum(x => x.Value.FileData.Length); }
        }

        public void CacheFile(string fileName, byte[] fileData, string transferEncoding = null)
        {
            cache[new Tuple<string, string>(fileName, transferEncoding)] = new CachableFileInfo(fileName, fileData, transferEncoding);

            if (log.IsDebugEnabled)
                log.DebugFormat(
                    CultureInfo.InvariantCulture,
                    "Caching file '{0}' (transf. enc.='{1}'), {2} ({3} total cached files)",
                    fileName,
                    transferEncoding,
                    FormattingUtils.FormatByteSizeRoundedToString(fileData.Length),
                    FormattingUtils.FormatByteSizeRoundedToString(CachedFilesTotalSize));
        }

        public bool TryGetFile(string fileName, string transferEncoding, out CachableFileInfo fileInfo)
        {
            return cache.TryGetValue(new Tuple<string, string>(fileName, transferEncoding), out fileInfo);
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<Tuple<string, string>, CachableFileInfo> cache = new Dictionary<Tuple<string, string>, CachableFileInfo>();
    }
}