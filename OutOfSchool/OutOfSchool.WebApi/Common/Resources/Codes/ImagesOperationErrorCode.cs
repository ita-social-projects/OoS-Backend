namespace OutOfSchool.WebApi.Common.Resources.Codes
{
    /// <summary>
    /// Contains error codes of all operations with images.
    /// </summary>
    public enum ImagesOperationErrorCode
    {
        /// <summary>
        /// The default error.
        /// </summary>
        [ResourcesKey(nameof(DefaultError))]
        DefaultError = 0,

        /// <summary>
        /// The error that can be used when something wrong has happened while uploading images.
        /// </summary>
        [ResourcesKey(nameof(UploadingError))]
        UploadingError = 1,

        /// <summary>
        /// The error that can be used when something wrong has happened while removing images.
        /// </summary>
        [ResourcesKey(nameof(RemovingError))]
        RemovingError = 2,

        /// <summary>
        /// The error that can be used when unable to make some operation because of an image storage.
        /// </summary>
        [ResourcesKey(nameof(ImageStorageError))]
        ImageStorageError = 3,

        /// <summary>
        /// The error that can be used when some image has not found.
        /// </summary>
        [ResourcesKey(nameof(ImageNotFoundError))]
        ImageNotFoundError = 4,

        /// <summary>
        /// The error that can be used when some image hasn't passed the validation process because of unexpected factors.
        /// </summary>
        [ResourcesKey(nameof(UnexpectedValidationError))]
        UnexpectedValidationError = 5,

        /// <summary>
        /// The error that can be used when some image hasn't passed the validation process because of the invalid image size.
        /// </summary>
        [ResourcesKey(nameof(InvalidSizeError))]
        InvalidSizeError = 6,

        /// <summary>
        /// The error that can be used when some image hasn't passed the validation process because of the invalid image format.
        /// </summary>
        [ResourcesKey(nameof(InvalidFormatError))]
        InvalidFormatError = 7,

        /// <summary>
        /// The error that can be used when some image hasn't passed the validation process because of the invalid image resolution.
        /// </summary>
        [ResourcesKey(nameof(InvalidResolutionError))]
        InvalidResolutionError = 8,

        /// <summary>
        /// The error that can be used when cannot find the entity for operations with images.
        /// </summary>
        [ResourcesKey(nameof(EntityNotFoundError))]
        EntityNotFoundError = 9,

        /// <summary>
        /// The error that can be used when no images were given for making operations.
        /// </summary>
        [ResourcesKey(nameof(NoGivenImagesError))]
        NoGivenImagesError = 10,

        /// <summary>
        /// The error that can be used when something wrong has happened while updating the entity with changed images.
        /// </summary>
        [ResourcesKey(nameof(UpdateEntityError))]
        UpdateEntityError = 11,

        /// <summary>
        /// The error that can be used when count of images is more than allowed for this type of entity.
        /// </summary>
        [ResourcesKey(nameof(ExceedingCountOfImagesError))]
        ExceedingCountOfImagesError = 12,
    }
}
