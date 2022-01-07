namespace OutOfSchool.WebApi.Common.Resources.Codes
{
    public enum ImagesOperationErrorCode
    {
        [ResourcesKey(nameof(DefaultError))]
        DefaultError = 0,

        [ResourcesKey(nameof(UploadingError))]
        UploadingError = 1,

        [ResourcesKey(nameof(RemovingError))]
        RemovingError = 2,

        [ResourcesKey(nameof(ImageStorageError))]
        ImageStorageError = 3,

        [ResourcesKey(nameof(ImageNotFoundError))]
        ImageNotFoundError = 4,

        [ResourcesKey(nameof(UnexpectedValidationError))]
        UnexpectedValidationError = 5,

        [ResourcesKey(nameof(InvalidSizeError))]
        InvalidSizeError = 6,

        [ResourcesKey(nameof(InvalidFormatError))]
        InvalidFormatError = 7,

        [ResourcesKey(nameof(InvalidResolutionError))]
        InvalidResolutionError = 8,

        [ResourcesKey(nameof(EntityNotFoundError))]
        EntityNotFoundError = 9,

        [ResourcesKey(nameof(NoGivenImagesError))]
        NoGivenImagesError = 10,

        [ResourcesKey(nameof(UpdateEntityError))]
        UpdateEntityError = 11,

        [ResourcesKey(nameof(ExceedingCountOfImagesError))]
        ExceedingCountOfImagesError = 12,
    }
}
