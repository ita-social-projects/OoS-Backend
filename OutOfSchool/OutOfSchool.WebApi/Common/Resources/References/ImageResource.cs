using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

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

        internal string DefaultError => GetResourceString(nameof(DefaultError));

        internal string UploadingError => GetResourceString(nameof(UploadingError));

        internal string RemovingError => GetResourceString(nameof(RemovingError));

        internal string ImageStorageError => GetResourceString(nameof(ImageStorageError));

        internal string ImageNotFoundError => GetResourceString(nameof(ImageNotFoundError));

        internal string UnexpectedValidationError => GetResourceString(nameof(UnexpectedValidationError));

        internal string InvalidSizeError => GetResourceString(nameof(InvalidSizeError));

        internal string InvalidFormatError => GetResourceString(nameof(InvalidFormatError));

        internal string InvalidResolutionError => GetResourceString(nameof(InvalidResolutionError));

        internal string EntityNotFoundError => GetResourceString(nameof(EntityNotFoundError));

        internal string NoGivenImagesError => GetResourceString(nameof(NoGivenImagesError));

        internal string UpdateEntityError => GetResourceString(nameof(UpdateEntityError));

        internal string ExceedingCountOfImagesError(int countOfImages)
        {
            return string.Format(CultureInfo.CurrentCulture, GetResourceString(nameof(ExceedingCountOfImagesError)), countOfImages);
        }
    }
}
