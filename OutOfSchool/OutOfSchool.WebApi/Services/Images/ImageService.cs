using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for operations with images.
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IExternalImageStorage externalStorage;
        private readonly IImageRepository imageRepository;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ImageService> logger;

        public ImageService(IExternalImageStorage externalStorage, IWorkshopRepository workshopRepository, IImageRepository imageRepository, IServiceProvider serviceProvider, ILogger<ImageService> logger)
        {
            this.externalStorage = externalStorage;
            this.imageRepository = imageRepository;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Result<ImageDto>> GetByIdAsync(string imageId)
        {
            if (imageId == null)
            {
                return Result<ImageDto>.Failed(new OperationError { Code = ImageResourceCodes.NotFoundError, Description = Resources.ImageResource.NotFoundError });
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
                return Result<ImageDto>.Failed(new OperationError { Code = ImageResourceCodes.NotFoundError, Description = Resources.ImageResource.NotFoundError });
            }
        }

        /// <inheritdoc/>
        // TODO: check the workshop's images limit in order to prevent uploading too many images into 1 workshop
        public async Task<MultipleKeyValueOperationResult> UploadManyWorkshopImagesWithUpdatingEntityAsync(
            Guid workshopId,
            List<IFormFile> fileCollection)
        {
            if (fileCollection == null || fileCollection.Count <= 0)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = Resources.ImageResource.NoImagesForUploading };
            }

            logger.LogDebug($"Uploading {fileCollection.Count} images for workshopId = {workshopId} was started.");
            var workshopRepository = GetWorkshopRepository();
            var validator = GetValidator<Workshop>();
            var workshop = await workshopRepository.GetById(workshopId).ConfigureAwait(false);

            if (workshop == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = Resources.ImageResource.WorkshopEntityNotFoundWhileUploadingError };
            }

            logger.LogDebug($"Workshop with id = {workshopId} was found.");

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
                        logger.LogError($"Image with {nameof(fileCollection)} id = {i} isn't valid.");
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
                logger.LogError(ex, $"Exception while uploading images for workshopId = {workshopId}: {ex.Message}");
                return new MultipleKeyValueOperationResult { GeneralResultMessage = Resources.ImageResource.UploadImagesError };
            }

            try
            {
                logger.LogDebug($"Started updating workshop [id = {workshopId}] with uploaded images.");
                savingExternalImageIds.ForEach(id => workshop.WorkshopImages.Add(new Image<Workshop> { ExternalStorageId = id }));
                await workshopRepository.Update(workshop).ConfigureAwait(false);
                logger.LogDebug($"Workshop [id = {workshopId}] was successfully updated.");
            }
            catch (Exception ex)
            {
                // TODO: mark image ids in order to delete
                logger.LogError(ex, $"Cannot update workshop with id = {workshopId} because of {ex.Message}");
                return new MultipleKeyValueOperationResult { GeneralResultMessage = Resources.ImageResource.UploadImagesError };
            }

            logger.LogInformation($"Uploading images for workshopId = {workshopId} was finished.");
            return uploadImageResults;
        }

        /// <inheritdoc/>
        // TODO: check the workshop's images limit in order to prevent uploading too many images into 1 workshop
        public async Task<OperationResult> UploadWorkshopImageWithUpdatingEntityAsync(Guid workshopId, ImageDto imageDto)
        {
            if (imageDto == null)
            {
                return OperationResult.Failed(new OperationError { Code = ImageResourceCodes.NoImagesForUploading, Description = Resources.ImageResource.NoImagesForUploading});
            }

            logger.LogDebug($"Uploading an image for workshopId = {workshopId} was started.");
            var workshopRepository = GetWorkshopRepository();
            var validator = GetValidator<Workshop>();
            var workshop = await workshopRepository.GetById(workshopId).ConfigureAwait(false);
            if (workshop == null)
            {
                return OperationResult.Failed(new OperationError{Code = ImageResourceCodes.WorkshopEntityNotFoundWhileUploadingError, Description = Resources.ImageResource.WorkshopEntityNotFoundWhileUploadingError});
            }

            logger.LogDebug($"Workshop [id = {workshopId}] was found.");
            string externalImageId;

            try
            {
                logger.LogDebug("Started validating process for a given image.");
                var validationResult = validator.Validate(imageDto.ContentStream);
                if (!validationResult.Succeeded)
                {
                    logger.LogError("Image isn't valid.");
                    return validationResult;
                }

                logger.LogDebug("Started uploading process into an external storage for a given image.");
                var imageUploadResult = await UploadImageProcessAsync(imageDto.ContentStream, imageDto.ContentType).ConfigureAwait(false);
                if (!imageUploadResult.Succeeded)
                {
                    logger.LogDebug("The image was successfully uploaded into an external storage.");
                    return imageUploadResult.OperationResult;
                }

                externalImageId = imageUploadResult.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Exception while uploading images for workshopId = {workshopId}: {ex.Message}");
                return OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UploadImagesError, Description = ImageResourceCodes.UploadImagesError });
            }

            try
            {
                logger.LogDebug($"Started updating workshop [id = {workshopId}] with uploaded images.");
                workshop.WorkshopImages.Add(new Image<Workshop> { ExternalStorageId = externalImageId});
                await workshopRepository.Update(workshop).ConfigureAwait(false);
                logger.LogDebug($"Workshop [id = {workshopId}] was successfully updated.");
            }
            catch (Exception ex)
            {
                // TODO: mark image id in order to delete
                logger.LogError(ex, $"Cannot update workshop with id = {workshopId} because of {ex.Message}");
                return OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UploadImagesError, Description = Resources.ImageResource.UploadImagesError });
            }

            logger.LogInformation($"Uploading an image for workshopId = {workshopId} was finished.");
            return OperationResult.Success;
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
                return Result<string>.Failed(new OperationError { Code = ImageResourceCodes.ImageStorageError, Description = Resources.ImageResource.ImageStorageError });
            }
        }

        private IImageValidatorService<T> GetValidator<T>()
        {
            return (IImageValidatorService<T>)serviceProvider.GetService(typeof(IImageValidatorService<T>))
                ?? throw new NullReferenceException($"Unable to receive ImageValidatorService of type {nameof(T)}");
        }

        private IWorkshopRepository GetWorkshopRepository()
        {
            return (IWorkshopRepository)serviceProvider.GetService(typeof(IWorkshopRepository))
                   ?? throw new NullReferenceException(
                       $"Unable to receive service instance of {nameof(IWorkshopRepository)}");
        }
    }
}
