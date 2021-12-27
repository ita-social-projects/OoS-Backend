﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Common.Resources.Codes;

namespace OutOfSchool.WebApi.Common.Resources.References
{
    /// <summary>
    /// Used to get access to image resources.
    /// </summary>
    internal class ImageResource : Resource
    {
        internal static readonly ImageResource Instance = new ImageResource(ResourceManagers.ImageResourceManager);

        internal ImageResource(ResourceManager resourceManager)
            : base(resourceManager)
        {
        }

        internal string UploadImagesError => GetResourceString(ImageResourceCodes.UploadImagesError);
        internal string RemoveImagesError => GetResourceString(ImageResourceCodes.RemoveImagesError);

        internal string ImageStorageError => GetResourceString(ImageResourceCodes.ImageStorageError);

        internal string NotFoundError => GetResourceString(ImageResourceCodes.NotFoundError);

        internal string UnexpectedValidationError => GetResourceString(ImageResourceCodes.UnexpectedValidationError);

        internal string InvalidImageSizeError => GetResourceString(ImageResourceCodes.InvalidImageSizeError);

        internal string InvalidImageFormatError => GetResourceString(ImageResourceCodes.InvalidImageFormatError);

        internal string InvalidImageResolutionError => GetResourceString(ImageResourceCodes.InvalidImageResolutionError);

        internal string WorkshopEntityNotFoundError => GetResourceString(ImageResourceCodes.WorkshopEntityNotFoundError);

        internal string NoImagesForUploading => GetResourceString(ImageResourceCodes.NoImagesForUploading);

        internal string NoImagesForDeleting => GetResourceString(ImageResourceCodes.NoImagesForDeleting);
        internal string RemoveImageError => GetResourceString(ImageResourceCodes.RemoveImageError);
    }
}
