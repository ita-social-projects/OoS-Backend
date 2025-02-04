using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Common.Resources.Codes;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.BusinessLogic.Services.Images;

/// <summary>
/// Provides APIs for operations with images.
/// </summary>
public class ImageService : IImageService
{
    private readonly IImageStorage imageStorage;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ImageService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageService"/> class.
    /// </summary>
    /// <param name="imageStorage">Storage for images.</param>
    /// <param name="serviceProvider">Provides access to the app services.</param>
    /// <param name="logger">Logger.</param>
    public ImageService(
        IImageStorage imageStorage,
        IServiceProvider serviceProvider,
        ILogger<ImageService> logger)
    {
        this.imageStorage = imageStorage;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ImageDto>> GetByIdAsync(string imageId)
    {
        if (string.IsNullOrEmpty(imageId))
        {
            return Result<ImageDto>.Failed(ImagesOperationErrorCode.ImageNotFoundError.GetOperationError());
        }

        logger.LogDebug("Getting image by id='{ImageId}' was started", imageId);

        try
        {
            var externalImageModel = await imageStorage.GetByIdAsync(imageId).ConfigureAwait(false);

            if (externalImageModel == null)
            {
                logger.LogDebug("Image with id='{ImageId}' wasn't found", imageId);
                return Result<ImageDto>.Failed(ImagesOperationErrorCode.ImageNotFoundError.GetOperationError());
            }

            var imageDto = new ImageDto
            {
                ContentStream = externalImageModel.ContentStream, ContentType = externalImageModel.ContentType,
            };

            logger.LogDebug("Image with id='{ImageId}' was successfully got", imageId);
            return Result<ImageDto>.Success(imageDto);
        }
        catch (FileStorageException ex)
        {
            logger.LogError(ex, "Image with id='{ImageId}' wasn't found", imageId);
            return Result<ImageDto>.Failed(ImagesOperationErrorCode.ImageNotFoundError.GetOperationError());
        }
    }

    /// <inheritdoc/>
    public async Task<MultipleImageUploadingResult> UploadManyImagesAsync<TEntity>(IList<IFormFile> images)
    {
        if (images == null || images.Count <= 0)
        {
            throw new ArgumentException(@"Given images must be a non-null and not empty collection.", nameof(images));
        }

        logger.LogTrace("Uploading images was started");
        var validator = GetValidator<TEntity>();

        var savingExternalImageIds = new List<string>();
        var uploadingImagesResults = new MultipleKeyValueOperationResult();

        for (short i = 0; i < images.Count; i++)
        {
            try
            {
                logger.LogTrace("Started uploading process for index number {Index}", i);
                await using var stream = images[i].OpenReadStream();

                var validationResult = validator.Validate(stream);
                if (!validationResult.Succeeded)
                {
                    uploadingImagesResults.Results.Add(i, validationResult);
                    continue;
                }

                var imageUploadResult =
                    await UploadImageProcessAsync(stream, images[i].ContentType).ConfigureAwait(false);
                uploadingImagesResults.Results.Add(i, imageUploadResult.OperationResult);
                if (imageUploadResult.Succeeded)
                {
                    logger.LogTrace("Image with index number {Index} was successfully uploaded into a storage", i);
                    savingExternalImageIds.Add(imageUploadResult.Value);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while uploading images");
                uploadingImagesResults.Results.TryAdd(i, OperationResult.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError()));
            }
        }

        logger.LogTrace("Uploading images was finished");
        return new MultipleImageUploadingResult
        {
            SavedIds = savingExternalImageIds,
            MultipleKeyValueOperationResult = uploadingImagesResults,
        };
    }

    /// <inheritdoc/>
    public async Task<Result<string>> UploadImageAsync<TEntity>(IFormFile image)
    {
        _ = image ?? throw new ArgumentNullException(nameof(image));

        logger.LogTrace("Uploading an image was started");
        var validator = GetValidator<TEntity>();
        string externalImageId;

        try
        {
            await using var stream = image.OpenReadStream();
            var validationResult = validator.Validate(stream);
            if (!validationResult.Succeeded)
            {
                return Result<string>.Failed(validationResult.Errors.ToArray());
            }

            var imageUploadResult = await UploadImageProcessAsync(stream, image.ContentType).ConfigureAwait(false);
            if (!imageUploadResult.Succeeded)
            {
                return imageUploadResult;
            }

            externalImageId = imageUploadResult.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while uploading images");
            return Result<string>.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError());
        }

        logger.LogTrace("Uploading an image was successfully finished");
        return Result<string>.Success(externalImageId);
    }

    /// <inheritdoc/>
    public async Task<MultipleImageRemovingResult> RemoveManyImagesAsync(IList<string> imageIds)
    {
        if (imageIds == null || imageIds.Count <= 0)
        {
            throw new ArgumentException(@"Given images must be a non-null and not empty collection.", nameof(imageIds));
        }

        logger.LogTrace("Removing images was started");
        var removingExternalImageIds = new List<string>();
        var removingImagesResults = new MultipleKeyValueOperationResult();

        for (short i = 0; i < imageIds.Count; i++)
        {
            var imageRemovingResult = await RemovingImageProcessAsync(imageIds[i]).ConfigureAwait(false);
            removingImagesResults.Results.Add(i, imageRemovingResult);
            if (imageRemovingResult.Succeeded)
            {
                logger.LogTrace("Image with id {ImageId} was successfully deleted", imageIds[i]);
                removingExternalImageIds.Add(imageIds[i]);
            }
        }

        logger.LogTrace("Removing images was finished");
        return new MultipleImageRemovingResult
        {
            RemovedIds = removingExternalImageIds,
            MultipleKeyValueOperationResult = removingImagesResults,
        };
    }

    /// <inheritdoc/>
    public async Task<OperationResult> RemoveImageAsync(string imageId)
    {
        if (string.IsNullOrEmpty(imageId))
        {
            throw new ArgumentException(@"Image id must be a non empty string", nameof(imageId));
        }

        logger.LogTrace("Deleting an image with imageId {ImageId} was started", imageId);
        return await RemovingImageProcessAsync(imageId).ConfigureAwait(false);
    }

    private async Task<Result<string>> UploadImageProcessAsync(Stream contentStream, string contentType)
    {
        try
        {
            var imageStorageId = await imageStorage
                .UploadAsync(
                    new ImageFileModel { ContentStream = contentStream, ContentType = contentType },
                    Constants.PublicImageCacheControl,
                    new Dictionary<string, string>
                    {
                        {"Content-Disposition", "inline"}
                    })
                .ConfigureAwait(false);

            return Result<string>.Success(imageStorageId);
        }
        catch (FileStorageException ex)
        {
            logger.LogError(ex, "Unable to upload image into a storage");
            return Result<string>.Failed(ImagesOperationErrorCode.ImageStorageError.GetOperationError());
        }
    }

    private async Task<OperationResult> RemovingImageProcessAsync(string imageId)
    {
        if (string.IsNullOrEmpty(imageId))
        {
            return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
        }

        try
        {
            await imageStorage.DeleteAsync(imageId).ConfigureAwait(false);
            return OperationResult.Success;
        }
        catch (FileStorageException ex)
        {
            logger.LogError(ex, "Unreal to delete image from the storage");
            return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
        }
    }

    private IImageValidator<T> GetValidator<T>()
    {
        return serviceProvider.GetService<IImageValidator<T>>() ??
               throw new NullReferenceException($"Unable to receive ImageValidatorService of type {nameof(T)}");
    }
}