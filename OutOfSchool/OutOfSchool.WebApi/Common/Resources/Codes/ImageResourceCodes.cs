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
        internal const string UploadImagesError = nameof(UploadImagesError);
        internal const string ImageStorageError = nameof(ImageStorageError);
        internal const string NotFoundError = nameof(NotFoundError);
        internal const string UnexpectedValidationError = nameof(UnexpectedValidationError);
        internal const string InvalidImageSizeError = nameof(InvalidImageSizeError);
        internal const string InvalidImageFormatError = nameof(InvalidImageFormatError);
        internal const string InvalidImageResolutionError = nameof(InvalidImageResolutionError);
        internal const string WorkshopEntityNotFoundWhileUploadingError = nameof(WorkshopEntityNotFoundWhileUploadingError);
        internal const string NoImagesForUploading = nameof(NoImagesForUploading);
    }
}
