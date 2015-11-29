using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Syborg.Common;

namespace Syborg
{
    public class FileMimeTypesMap : IFileMimeTypesMap
    {
        public FileMimeTypesMap RegisterStandardMimeTypes()
        {
            RegisterMimeType(".css", HttpConsts.ContentTypeTextCss);
            RegisterMimeType(".eot", HttpConsts.ContentTypeApplicationVndMsFontObjects);
            RegisterMimeType(".jpg", HttpConsts.ContentTypeImageJpeg);
            RegisterMimeType(".js", HttpConsts.ContentTypeApplicationXJavaScript);
            RegisterMimeType(".png", HttpConsts.ContentTypeImagePng);
            RegisterMimeType(".svg", HttpConsts.ContentTypeImageSvg);
            RegisterMimeType(".ttf", HttpConsts.ContentTypeApplicationXFontTtf);
            RegisterMimeType(".woff", HttpConsts.ContentTypeApplicationXFontWoff);
            RegisterMimeType(".woff2", HttpConsts.ContentTypeApplicationXFontWoff);

            return this;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase"), System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void RegisterMimeType(string fileExtension, string mimeType)
        {
            string fileExtensionLowercase = fileExtension.ToLower(CultureInfo.InvariantCulture);

            if (mimeTypes.ContainsKey(fileExtensionLowercase))
                throw new InvalidOperationException("File extension '{0}' already registered as mime type".Fmt(fileExtension));

            mimeTypes.Add (fileExtensionLowercase, mimeType);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public string GetContentType(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName);
            // ReSharper disable once PossibleNullReferenceException
            fileExtension = fileExtension.ToLower(CultureInfo.InvariantCulture);

            string mimeType;
            if (mimeTypes.TryGetValue(fileExtension, out mimeType))
                return mimeType;

            return null;
        }

        private readonly Dictionary<string, string> mimeTypes = new Dictionary<string, string>();
    }
}