using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources.Codes;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    // TODO: make synchronization to remove incorrect operations' ids

    /// <summary>
    /// Represents a base mediator class for operations with images.
    /// This instance does not save the given entity changes.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public class ImageInteractionBaseMediator<TEntity> : IImageInteractionMediator<TEntity>
        where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInteractionBaseMediator{TEntity}"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="limits">Describes limits of images for entities.</param>
        /// <param name="logger">Logger.</param>
        public ImageInteractionBaseMediator(
            IImageService imageService,
            ImagesLimits<TEntity> limits,
            ILogger<ImageInteractionBaseMediator<TEntity>> logger)
        {
            ImageService = imageService;
            Limits = limits;
            Logger = logger;
        }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected ILogger<ImageInteractionBaseMediator<TEntity>> Logger { get; }

        /// <summary>
        /// Gets a service for interacting with an image storage..
        /// </summary>
        protected IImageService ImageService { get; }

        /// <summary>
        /// Gets limits of images for entity of <c>TEntity</c> type.
        /// </summary>
        protected ImagesLimits<TEntity> Limits { get; }

        /// <inheritdoc/>
        public async Task<Result<string>> UploadImageAsync(TEntity entity, IFormFile image)
            => await UploadImageProcessAsync(entity, image).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<OperationResult> RemoveImageAsync(TEntity entity, string imageId)
            => await RemoveImageProcessAsync(entity, imageId).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<ImageUploadingResult> UploadManyImagesAsync(TEntity entity, IList<IFormFile> images)
            => await UploadManyImagesProcessAsync(entity, images).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<ImageRemovingResult> RemoveManyImagesAsync(TEntity entity, IList<string> imageIds)
            => await RemoveManyImagesProcessAsync(entity, imageIds).ConfigureAwait(false);

        /// <summary>
        /// Determines if the given count of images is allowed for this entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="countOfFiles">Count of files.</param>
        /// <returns>The <see cref="bool"/> value which shows if uploading is allowed.</returns>
        protected virtual bool AllowedToUploadGivenAmountOfFiles(TEntity entity, int countOfFiles)
        {
            _ = entity?.Images ?? throw new ArgumentException(@"Unable to access images because it's null for the given entity.", nameof(entity));
            return entity.Images.Count + countOfFiles <= Limits.MaxCountOfFiles;
        }

        /// <summary>
        /// The process of uploading an image for the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="image">Image for uploading.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="OperationResult"/>.</returns>
        /// <exception cref="ArgumentNullException">When entity or image id is null.</exception>
        private protected async Task<Result<string>> UploadImageProcessAsync(
            TEntity entity,
            IFormFile image)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            _ = image ?? throw new ArgumentNullException(nameof(image));

            Logger.LogTrace("Uploading an image for the entity was started");
            if (!AllowedToUploadGivenAmountOfFiles(entity, 1))
            {
                Logger.LogTrace("The image limit was reached for the entity");
                return Result<string>.Failed(ImagesOperationErrorCode.ExceedingCountOfImagesError.GetOperationError());
            }

            var imageUploadingResult = await ImageService.UploadImageAsync<TEntity>(image).ConfigureAwait(false);
            if (!imageUploadingResult.Succeeded)
            {
                return Result<string>.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError());
            }

            entity.Images.Add(new Image<TEntity> { ExternalStorageId = imageUploadingResult.Value });

            Logger.LogTrace("Uploading an image for the entity was finished");
            return imageUploadingResult;
        }

        /// <summary>
        /// The process of removing an image from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageId">Image Id for removing.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="OperationResult"/>.</returns>
        /// <exception cref="ArgumentNullException">When entity or image id is null.</exception>
        private protected async Task<OperationResult> RemoveImageProcessAsync(
            TEntity entity,
            string imageId)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrEmpty(imageId))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Logger.LogTrace("Removing an image for the entity was started");
            var ableToRemove = entity.Images.Select(x => x.ExternalStorageId).Contains(imageId);

            if (!ableToRemove)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            var imageRemovingResult = await ImageService.RemoveImageAsync(imageId).ConfigureAwait(false);

            if (!imageRemovingResult.Succeeded)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            RemoveImageFromEntity(entity, imageId);

            Logger.LogTrace("Removing an image for the entity was finished");

            return imageRemovingResult;
        }

        /// <summary>
        /// The process of uploading images for the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="images">Images for uploading.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
        /// <exception cref="ArgumentNullException">When an entity is null.</exception>
        /// <exception cref="ArgumentException">When a given list of images is null or empty.</exception>
        private protected async Task<ImageUploadingResult> UploadManyImagesProcessAsync(
            TEntity entity,
            IList<IFormFile> images)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));

            if (images == null || images.Count <= 0)
            {
                throw new ArgumentException(@"Given images must be non-null and not empty.", nameof(images));
            }

            Logger.LogTrace("Uploading images for the entity was started");
            if (!AllowedToUploadGivenAmountOfFiles(entity, images.Count))
            {
                Logger.LogTrace("The image limit was reached for the entity");
                return new ImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.ExceedingCountOfImagesError.GetResourceValue() } };
            }

            var imagesUploadingResult = await ImageService.UploadManyImagesAsync<TEntity>(images).ConfigureAwait(false);
            if (imagesUploadingResult.SavedIds == null || imagesUploadingResult.MultipleKeyValueOperationResult == null)
            {
                return new ImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.UploadingError.GetResourceValue() } };
            }

            if (imagesUploadingResult.SavedIds.Count > 0)
            {
                entity.Images.AddRange(imagesUploadingResult.SavedIds.Select(id => new Image<TEntity> { ExternalStorageId = id }));
            }

            Logger.LogTrace("Uploading images for the entity was finished");
            return imagesUploadingResult;
        }

        /// <summary>
        /// The process of removing images from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageIds">Image ids for removing.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
        private protected async Task<ImageRemovingResult> RemoveManyImagesProcessAsync(
            TEntity entity,
            IList<string> imageIds)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));

            if (imageIds == null || imageIds.Count <= 0)
            {
                throw new ArgumentException(@"Given images must be non-null and not empty.", nameof(imageIds));
            }

            Logger.LogTrace("Removing images for the entity was started");
            var ableToRemove = !imageIds.Except(entity.Images.Select(x => x.ExternalStorageId)).Any();

            if (!ableToRemove)
            {
                return new ImageRemovingResult
                {
                    MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.RemovingError.GetResourceValue() },
                };
            }

            var imagesRemovingResult = await ImageService.RemoveManyImagesAsync(imageIds).ConfigureAwait(false);

            if (imagesRemovingResult.RemovedIds == null || imagesRemovingResult.MultipleKeyValueOperationResult == null)
            {
                return new ImageRemovingResult
                {
                    MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.RemovingError.GetResourceValue() },
                };
            }

            if (imagesRemovingResult.RemovedIds.Count > 0)
            {
                imagesRemovingResult.RemovedIds.ForEach(x => RemoveImageFromEntity(entity, x));
            }

            Logger.LogTrace("Removing images for the entity was finished");
            return imagesRemovingResult;
        }

        private static void RemoveImageFromEntity(TEntity entity, string imageId)
        {// try to use except
            entity.Images.RemoveAt(
                entity.Images.FindIndex(i => i.ExternalStorageId == imageId));
        }
    }
}
