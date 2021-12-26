using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OutOfSchool.Services.CombinedProviders;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Codes;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Extensions;
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

        public ImageService(IExternalImageStorage externalStorage, IServiceProvider serviceProvider, ILogger<ImageService> logger)
        {
            this.externalStorage = externalStorage;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Result<ImageDto>> GetByIdAsync(string imageId)
        {
            if (imageId == null)
            {
                return Result<ImageDto>.Failed(new OperationError { Code = ImageResourceCodes.NotFoundError, Description = ResourceInstances.ImageResource.NotFoundError });
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

                logger.LogInformation($"Image with id {imageId} was successfully got");
                return Result<ImageDto>.Success(imageDto);
            }
            catch (ImageStorageException ex)
            {
                return Result<ImageDto>.Failed(new OperationError { Code = ImageResourceCodes.NotFoundError, Description = ResourceInstances.ImageResource.NotFoundError });
            }
        }

        /// <inheritdoc/>
        // TODO: check the workshop's images limit in order to prevent uploading too many images into 1 workshop
        public async Task<ImageUploadingResult> UploadManyImagesAsync<TEntity>(List<IFormFile> fileCollection)
        {
            if (fileCollection == null || fileCollection.Count <= 0)
            {
                return new ImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.NoImagesForUploading } };
            }

            logger.LogDebug($"Uploading {fileCollection.Count} images for was started.");
            var validator = GetValidator<TEntity>();

            var savingExternalImageIds = new List<string>();
            var uploadImageResults = new MultipleKeyValueOperationResult();

            try
            {
                for (short i = 0; i < fileCollection.Count; i++)
                {
                    logger.LogDebug($"Started uploading process for {nameof(fileCollection)} id number {i}.");
                    await using var stream = fileCollection[i].OpenReadStream();

                    logger.LogDebug($"Started validating process for {nameof(fileCollection)} id number {i}.");
                    var validationResult = validator.Validate(stream);
                    if (!validationResult.Succeeded)
                    {
                        logger.LogError($"Image with {nameof(fileCollection)} id = {i} isn't valid: {string.Join(",", validationResult.Errors)}");
                        uploadImageResults.Results.Add(i, validationResult);
                        continue;
                    }

                    logger.LogDebug(
                        $"Started uploading process into an external storage for {nameof(fileCollection)} id number {i}.");
                    var imageUploadResult = await UploadImageProcessAsync(stream, fileCollection[i].ContentType).ConfigureAwait(false);
                    uploadImageResults.Results.Add(i, imageUploadResult.OperationResult);
                    if (imageUploadResult.Succeeded)
                    {
                        logger.LogDebug(
                            $"Image with {nameof(fileCollection)} id number {i} was successfully uploaded into an external storage.");
                        savingExternalImageIds.Add(imageUploadResult.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception while uploading images, message: {ex.Message}");
                return new ImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.UploadImagesError } };
            }

            logger.LogInformation($"Uploading {fileCollection.Count} images was finished.");
            return new ImageUploadingResult
                { SavedIds = savingExternalImageIds, MultipleKeyValueOperationResult = uploadImageResults };
        }

        /// <inheritdoc/>
        // TODO: check the workshop's images limit in order to prevent uploading too many images into 1 workshop
        public async Task<Result<string>> UploadImageAsync<TEntity>(ImageDto imageDto)
        {
            if (imageDto == null)
            {
                return Result<string>.Failed(new OperationError { Code = ImageResourceCodes.NoImagesForUploading, Description = ResourceInstances.ImageResource.NoImagesForUploading });
            }

            logger.LogDebug("Uploading an image was started.");
            var validator = GetValidator<TEntity>();
            string externalImageId;

            try
            {
                logger.LogDebug("Started validating process for a given image.");
                var validationResult = validator.Validate(imageDto.ContentStream);
                if (!validationResult.Succeeded)
                {
                    logger.LogError("Image isn't valid.");
                    return Result<string>.Failed(validationResult.Errors.ToArray());
                }

                logger.LogDebug("Started uploading process into an external storage for a given image.");
                var imageUploadResult = await UploadImageProcessAsync(imageDto.ContentStream, imageDto.ContentType).ConfigureAwait(false);
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
                return Result<string>.Failed(new OperationError { Code = ImageResourceCodes.UploadImagesError, Description = ImageResourceCodes.UploadImagesError });
            }

            logger.LogInformation("Uploading an image was successfully finished.");
            return Result<string>.Success(externalImageId);
        }

        public async Task<OperationResult> RemoveWorkshopImagesByIdsAsync(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        private async Task<Result<string>> UploadImageProcessAsync(Stream contentStream, string contentType)
        {
            try
            {
                var imageStorageId = await externalStorage.UploadImageAsync(new ExternalImageModel {ContentStream = contentStream, ContentType = contentType})
                    .ConfigureAwait(false);

                logger.LogDebug("Uploading into an external storage result: Success.");
                return Result<string>.Success(imageStorageId);
            }
            catch (ImageStorageException ex)
            {
                logger.LogError(ex, $"Unable to upload image into an external storage because of {ex.Message}.");
                return Result<string>.Failed(new OperationError { Code = ImageResourceCodes.ImageStorageError, Description = ResourceInstances.ImageResource.ImageStorageError });
            }
        }

        private IImageValidatorService<T> GetValidator<T>()
        {
            return (IImageValidatorService<T>)serviceProvider.GetService(typeof(IImageValidatorService<T>))
                ?? throw new NullReferenceException($"Unable to receive ImageValidatorService of type {nameof(T)}");
        }
    }
}
