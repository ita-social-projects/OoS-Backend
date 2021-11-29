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
        public async Task<Result<ImageStorageModel>> GetByIdAsync(Guid imageId)
        {
            logger.LogInformation($"Getting image by id = {imageId} was started.");
            var imageMetadata = await imageRepository.GetMetadataById(imageId).ConfigureAwait(false);
            if (!IsImageMetadataValid(imageMetadata))
            {
                return Result<ImageStorageModel>.Failed(new OperationError { Code = ImageResourceCodes.NotFoundError, Description = Resources.ImageResource.NotFoundError });
            }

            try
            {
                var contentStream = await externalStorage.GetByIdAsync(imageMetadata.StorageId).ConfigureAwait(false);

                var imageStorageModel = new ImageStorageModel
                {
                    ContentStream = contentStream,
                    ContentType = imageMetadata.ContentType,
                };

                logger.LogInformation($"Image with id {imageId} was successfully got");
                return Result<ImageStorageModel>.Success(imageStorageModel);
            }
            catch (ImageStorageException ex)
            {
                return Result<ImageStorageModel>.Failed(new OperationError { Code = ImageResourceCodes.NotFoundError, Description = Resources.ImageResource.NotFoundError });
            }
        }

        // compare this variant with below

        /// <inheritdoc/>
        public async Task<IDictionary<short, OperationResult>> UploadManyWorkshopImagesWithUpdatingEntityAsync(
            Guid workshopId,
            List<IFormFile> fileCollection)
        {
            _ = fileCollection ?? throw new ArgumentNullException(nameof(fileCollection));

            logger.LogInformation($"Uploading {fileCollection.Count} images for workshopId = {workshopId} was started.");
            var workshopRepository = GetWorkshopRepository();
            var validator = GetValidator<Workshop>();
            var workshop = await workshopRepository.GetById(workshopId).ConfigureAwait(false);
            var savingMetadata = new List<ImageMetadata>();
            var uploadImageResults = new Dictionary<short, OperationResult>();

            try
            {
                ValidateEntity(workshop);
                logger.LogInformation($"Workshop with id = {workshopId} was found.");

                for (short i = 0; i < fileCollection.Count; i++)
                {
                    logger.LogInformation($"Started uploading process for {nameof(fileCollection)} id number {i}.");
                    await using var stream = fileCollection[i].OpenReadStream();

                    logger.LogInformation($"Started validating process for {nameof(fileCollection)} id number {i}.");
                    var validationResult = validator.Validate(stream);
                    if (!validationResult.Succeeded)
                    {
                        logger.LogError($"Image with {nameof(fileCollection)} id = {i} isn't valid.");
                        uploadImageResults.Add(i, validationResult);
                        continue;
                    }

                    logger.LogInformation(
                        $"Started uploading process into an external storage for {nameof(fileCollection)} id number {i}.");
                    var imageUploadResult = await UploadImageProcess(stream).ConfigureAwait(false);
                    uploadImageResults.Add(i, validationResult);
                    if (imageUploadResult.Succeeded)
                    {
                        logger.LogInformation(
                            $"Image with {nameof(fileCollection)} id number {i} was successfully uploaded into an external storage.");
                        savingMetadata.Add(new ImageMetadata
                        {
                            ContentType = fileCollection[i].ContentType,
                            StorageId = imageUploadResult.Value,
                        });
                    }
                }
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError($"Cannot find entity [workshopId = {workshopId}] in order to upload images, message: {ex.Message}");
                return new Dictionary<short, OperationResult> { { -1, OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UpdateImagesError, Description = ImageResourceCodes.UpdateImagesError }) } };
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception while uploading images for workshopId = {workshopId}: {ex.Message}");
                return new Dictionary<short, OperationResult> { { -1, OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UpdateImagesError, Description = ImageResourceCodes.UpdateImagesError }) } };
            }

            try
            {
                logger.LogInformation($"Started updating workshop [id = {workshopId}] with uploaded images.");
                savingMetadata.ForEach(metadata => workshop.WorkshopImages.Add(new Image<Workshop> { ImageMetadata = metadata }));
                await workshopRepository.Update(workshop).ConfigureAwait(false);
                logger.LogInformation($"Workshop [id = {workshopId}] was successfully updated.");
            }
            catch (Exception ex)
            {
                // TODO: mark image ids in order to delete
                logger.LogError($"Cannot update workshop with id = {workshopId} because of {ex.Message}");
                return new Dictionary<short, OperationResult> { { -1, OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UpdateImagesError, Description = ImageResourceCodes.UpdateImagesError }) } };
            }

            logger.LogInformation($"Uploading images for workshopId = {workshopId} was finished.");
            return uploadImageResults;
        }

        // compare this variant with upper

        /// <inheritdoc/>
        public async Task<OperationResult> UploadWorkshopImageWithUpdatingEntityAsync(Guid workshopId, ImageStorageModel imageModel)
        {
            _ = imageModel ?? throw new ArgumentNullException(nameof(imageModel));

            logger.LogInformation($"Uploading an image for workshopId = {workshopId} was started.");
            var workshopRepository = GetWorkshopRepository();
            var validator = GetValidator<Workshop>();
            var workshop = await workshopRepository.GetById(workshopId).ConfigureAwait(false);
            ImageMetadata imageMetadata;

            try
            {
                ValidateEntity(workshop);
                logger.LogInformation($"Workshop [id = {workshopId}] was found.");

                logger.LogInformation("Started validating process for a given image.");
                var validationResult = validator.Validate(imageModel.ContentStream);
                if (!validationResult.Succeeded)
                {
                    logger.LogError("Image isn't valid.");
                    return validationResult;
                }

                logger.LogInformation("Started uploading process into an external storage for a given image.");
                var imageUploadResult = await UploadImageProcess(imageModel.ContentStream).ConfigureAwait(false);
                if (!imageUploadResult.Succeeded)
                {
                    logger.LogInformation("The image was successfully uploaded into an external storage.");
                    return imageUploadResult.OperationResult;
                }

                imageMetadata = new ImageMetadata
                {
                    ContentType = imageModel.ContentType,
                    StorageId = imageUploadResult.Value,
                };
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogError($"Cannot find entity [workshopId = {workshopId}] in order to upload images, message: {ex.Message}");
                return OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UpdateImagesError, Description = ImageResourceCodes.UpdateImagesError });
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception while uploading images for workshopId = {workshopId}: {ex.Message}");
                return OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UpdateImagesError, Description = ImageResourceCodes.UpdateImagesError });
            }

            try
            {
                logger.LogInformation($"Started updating workshop [id = {workshopId}] with uploaded images.");
                workshop.WorkshopImages.Add(new Image<Workshop> { ImageMetadata = imageMetadata });
                await workshopRepository.Update(workshop).ConfigureAwait(false);
                logger.LogInformation($"Workshop [id = {workshopId}] was successfully updated.");
            }
            catch (Exception ex)
            {
                // TODO: mark image id in order to delete
                logger.LogError($"Cannot update workshop with id = {workshopId} because of {ex.Message}");
                return OperationResult.Failed(new OperationError { Code = ImageResourceCodes.UpdateImagesError, Description = Resources.ImageResource.UpdateImagesError });
            }

            logger.LogInformation($"Uploading an image for workshopId = {workshopId} was finished.");
            return OperationResult.Success;
        }

        private static void ValidateEntity<T>(T entity)
        {
            if (entity == null)
            {
                throw new EntityNotFoundException($"{nameof(entity)}, type: {nameof(T)}");
            }
        }

        private static bool IsImageMetadataValid(ImageMetadata imageMetadata)
        {
            return imageMetadata != null;
        }

        private async Task<Result<string>> UploadImageProcess(Stream contentStream)
        {
            try
            {
                var imageStorageId = await externalStorage.UploadImageAsync(contentStream)
                    .ConfigureAwait(false);

                logger.LogInformation("Uploading into an external storage result: Success.");
                return Result<string>.Success(imageStorageId);
            }
            catch (ImageStorageException ex)
            {
                logger.LogError($"Unable to upload image into an external storage because of {ex.Message}.");
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
