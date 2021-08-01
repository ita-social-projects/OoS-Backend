using System;
using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.WebApi.Services.PhotoStorage
{
    public static class MimeTypeMap
    {
        private static readonly Lazy<IDictionary<string, string>> Mappings = new Lazy<IDictionary<string, string>>(BuildMappings);

        public static string CurentContentType {get; private set; }

        public static string GetMimeType(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = "." + extension;
            }

            string mime;

            if (Mappings.Value.TryGetValue(extension, out mime))
            {
                CurentContentType = mime;
            }
            else
            {
                mime = "application/octet-stream";
            }

            return mime;
        }

        public static string GetExtension(string mimeType)
        {
            if (mimeType == null)
            {
                throw new ArgumentNullException(nameof(mimeType));
            }

            if (mimeType.StartsWith(".", StringComparison.Ordinal))
            {
                throw new ArgumentException("Requested mime type is not valid: " + mimeType);
            }

            string extension;

            if (Mappings.Value.TryGetValue(mimeType, out extension))
            {
                return extension;
            }

            throw new ArgumentException("Requested mime type is not registered: " + mimeType);
        }

        private static IDictionary<string, string> BuildMappings()
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {".bmp", "image/bmp"},
                {".gif", "image/gif"},
                {".jpe", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".png", "image/png"},
                {"image/bmp", ".bmp"},
                {"image/jpeg", ".jpg"},
                {"image/pict", ".pic"},
                {"image/png", ".png"},
                {"image/tiff", ".tiff"},
                {"image/x-macpaint", ".mac"},
                {"image/x-quicktime", ".qti"},
            };

            var cache = mappings.ToList();

            foreach (var mapping in cache)
            {
                if (!mappings.ContainsKey(mapping.Value))
                {
                    mappings.Add(mapping.Value, mapping.Key);
                }
            }

            return mappings;
        }
    }
}