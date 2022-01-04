using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Describers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for operations with images.
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IExternalImageStorage externalStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ImageService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageService"/> class.
        /// </summary>
        /// <param name="externalStorage">Storage for images.</param>
        /// <param name="serviceProvider">Provides access to the app services.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="errorDescriber">The instance which describes image errors.</param>
        public ImageService(IExternalImageStorage externalStorage, IServiceProvider serviceProvider, ILogger<ImageService> logger, ImagesErrorDescriber errorDescriber)
        {
            this.externalStorage = externalStorage;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            ErrorDescriber = errorDescriber;
        }

        /// <inheritdoc/>
        public ImagesErrorDescriber ErrorDescriber { get; }

        /// <inheritdoc/>
        public async Task<Result<ImageDto>> GetByIdAsync(string imageId)
        {
            if (imageId == null)
            {
                return Result<ImageDto>.Failed(ErrorDescriber.ImageNotFoundError());
            }

            logger.LogDebug($"Getting image by id = {imageId} was started.");

            try
            {
                var externalImageModel = await externalStorage.GetByIdAsync(imageId).ConfigureAwait(false);

                var imageDto = new ImageDto
                {
                    ContentStream = externalImageModel.ContentStream,
                    ContentType = externalImageModel.ContentType,
                };

                logger.LogDebug($"Image with id {imageId} was successfully got.");
                return Result<ImageDto>.Success(imageDto);
            }
            catch (ImageStorageException ex)
            {
                logger.LogError(ex, $"Image with id {imageId} wasn't found.");
                return Result<ImageDto>.Failed(ErrorDescriber.ImageNotFoundError());
            }
        }

        /// <inheritdoc/>
        public async Task<ImageUploadingResult> UploadManyImagesAsync<TEntity>(List<IFormFile> images)
        {
            if (images == null || images.Count <= 0)
            {
                return new ImageUploadingResult
                { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.NoGivenImagesError } };
            }

            logger.LogDebug($"Uploading {images.Count} images was started.");
            var validator = GetValidator<TEntity>();

            var savingExternalImageIds = new List<string>();
            var uploadingImagesResults = new MultipleKeyValueOperationResult();

            try
            {
                for (short i = 0; i < images.Count; i++)
                {
                    logger.LogDebug($"Started uploading process for {nameof(images)} id number {i}.");
                    await using var stream = images[i].OpenReadStream();

                    logger.LogDebug($"Started validating process for {nameof(images)} id number {i}.");
                    var validationResult = validator.Validate(stream);
                    if (!validationResult.Succeeded)
                    {
                        logger.LogError($"Image with {nameof(images)} id = {i} isn't valid: {string.Join(",", validationResult.Errors)}");
                        uploadingImagesResults.Results.Add(i, validationResult);
                        continue;
                    }

                    logger.LogDebug(
                        $"Started uploading process into an external storage for {nameof(images)} id number {i}.");
                    var imageUploadResult = await UploadImageProcessAsync(stream, images[i].ContentType).ConfigureAwait(false);
                    uploadingImagesResults.Results.Add(i, imageUploadResult.OperationResult);
                    if (imageUploadResult.Succeeded)
                    {
                        logger.LogDebug(
                            $"Image with {nameof(images)} id number {i} was successfully uploaded into an external storage.");
                        savingExternalImageIds.Add(imageUploadResult.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception while uploading images, message: {ex.Message}");
                return new ImageUploadingResult
                { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.UploadingError } };
            }

            logger.LogDebug($"Uploading {images.Count} images was finished.");
            return new ImageUploadingResult
            { SavedIds = savingExternalImageIds, MultipleKeyValueOperationResult = uploadingImagesResults };
        }

        /// <inheritdoc/>
        public async Task<Result<string>> UploadImageAsync<TEntity>(IFormFile image)
        {
            if (image == null)
            {
                return Result<string>.Failed(ErrorDescriber.NoGivenImagesError());
            }

            logger.LogDebug("Uploading an image was started.");
            var validator = GetValidator<TEntity>();
            string externalImageId;

            try
            {
                logger.LogDebug("Started validating process for a given image.");
                await using var stream = image.OpenReadStream();
                var validationResult = validator.Validate(stream);
                if (!validationResult.Succeeded)
                {
                    logger.LogError($"Image isn't valid: {string.Join(",", validationResult.Errors)}");
                    return Result<string>.Failed(validationResult.Errors.ToArray());
                }

                logger.LogDebug("Started uploading process into an external storage for a given image.");
                var imageUploadResult = await UploadImageProcessAsync(stream, image.ContentType).ConfigureAwait(false);
                if (!imageUploadResult.Succeeded)
                {
                    return imageUploadResult;
                }

                logger.LogDebug("The image was successfully uploaded into an external storage.");
                externalImageId = imageUploadResult.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception while uploading images, message: {ex.Message}");
                return Result<string>.Failed(ErrorDescriber.UploadingError());
            }

            logger.LogDebug("Uploading an image was successfully finished.");
            return Result<string>.Success(externalImageId);
        }

        /// <inheritdoc/>
        public async Task<ImageRemovingResult> RemoveManyImagesAsync(List<string> imageIds)
        {
            if (imageIds == null || imageIds.Count == 0)
            {
                return new ImageRemovingResult
                { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.NoGivenImagesError } };
            }

            logger.LogDebug($"Removing {imageIds.Count} images was started.");
            var removingExternalImageIds = new List<string>();
            var removingImagesResults = new MultipleKeyValueOperationResult();

            for (short i = 0; i < imageIds.Count; i++)
            {
                var imageRemovingResult = await RemovingImageProcessAsync(imageIds[i]).ConfigureAwait(false);
                removingImagesResults.Results.Add(i, imageRemovingResult);
                if (imageRemovingResult.Succeeded)
                {
                    logger.LogDebug($"Image with an external id = {imageIds[i]} was successfully deleted.");
                    removingExternalImageIds.Add(imageIds[i]);
                }
            }

            logger.LogDebug($"Removing {imageIds.Count} images was finished.");
            return new ImageRemovingResult { RemovedIds = removingExternalImageIds, MultipleKeyValueOperationResult = removingImagesResults };
        }

        /// <inheritdoc/>
        public async Task<OperationResult> RemoveImageAsync(string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                OperationResult.Failed(ErrorDescriber.RemovingError());
            }

            logger.LogDebug($"Deleting an image with external id = {imageId} was started.");
            return await RemovingImageProcessAsync(imageId).ConfigureAwait(false);
        }

        private async Task<Result<string>> UploadImageProcessAsync(Stream contentStream, string contentType)
        {
            try
            {
                var imageStorageId = await externalStorage.UploadImageAsync(new ExternalImageModel { ContentStream = contentStream, ContentType = contentType })
                    .ConfigureAwait(false);

                return Result<string>.Success(imageStorageId);
            }
            catch (ImageStorageException ex)
            {
                logger.LogError(ex, $"Unable to upload image into an external storage because of {ex.Message}.");
                return Result<string>.Failed(ErrorDescriber.ImageStorageError());
            }
        }

        private async Task<OperationResult> RemovingImageProcessAsync(string imageId)
        {
            try
            {
                await externalStorage.DeleteImageAsync(imageId).ConfigureAwait(false);
                return OperationResult.Success;
            }
            catch (ImageStorageException ex)
            {
                logger.LogError(ex, $"Unreal to delete image with an external id = {imageId}.");
                return OperationResult.Failed(ErrorDescriber.RemovingError());
            }
        }

        private IImageValidatorService<T> GetValidator<T>()
        {
            return (IImageValidatorService<T>)serviceProvider.GetService(typeof(IImageValidatorService<T>))
                ?? throw new NullReferenceException($"Unable to receive ImageValidatorService of type {nameof(T)}");
        }
    }
}
