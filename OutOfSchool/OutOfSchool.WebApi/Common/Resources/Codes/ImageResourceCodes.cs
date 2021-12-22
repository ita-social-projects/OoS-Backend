﻿using System;
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
        internal const string UploadImagesError = "UploadImagesError";
        internal const string ImageStorageError = "ImageStorageError";
        internal const string NotFoundError = "NotFoundError";
        internal const string UnexpectedValidationError = "UnexpectedValidationError";
        internal const string InvalidImageSizeError = "InvalidImageSizeError";
        internal const string InvalidImageFormatError = "InvalidImageFormatError";
        internal const string InvalidImageResolutionError = "InvalidImageResolutionError";
        internal const string WorkshopEntityNotFoundWhileUploadingError = "WorkshopEntityNotFoundWhileUploadingError";
        internal const string NoImagesForUploading = "NoImagesForUploading";
    }
}
