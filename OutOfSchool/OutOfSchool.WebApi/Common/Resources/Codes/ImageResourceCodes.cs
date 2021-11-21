using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common.Resources.Codes
{
    /// <summary>
    /// Contains all resource codes for images.
    /// </summary>
    internal static class ImageResourceCodes
    {
        internal const string UpdateImagesError = "UpdateImagesError";
        internal const string ImageStorageError = "ImageStorageError";
        internal const string UnexpectedValidationError = "UnexpectedValidationError";
        internal const string InvalidImageSizeError = "InvalidImageSizeError";
        internal const string InvalidImageFormatError = "InvalidImageFormatError";
        internal const string InvalidImageResolutionError = "InvalidImageResolutionError";
    }
}
