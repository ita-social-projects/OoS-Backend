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
    /// Represents a base class for operations with images.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TKey">The type of entity Id.</typeparam>
    public class ImageInteractionBaseService<TEntity, TKey> : IImageInteractionService<TKey>
        where TEntity : class, IKeyedEntity<TKey>, IImageDependentEntity<TEntity>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInteractionBaseService{TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="repository">Repository with images.</param>
        /// <param name="limits">Describes limits of images for entities.</param>
        /// <param name="logger">Logger.</param>
        public ImageInteractionBaseService(
            IImageService imageService,
            IEntityRepositoryBase<TKey, TEntity> repository,
            ImagesLimits<TEntity> limits,
            ILogger<ImageInteractionBaseService<TEntity, TKey>> logger)
        {
            ImageService = imageService;
            Repository = repository;
            Limits = limits;
            Logger = logger;
        }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected ILogger<ImageInteractionBaseService<TEntity, TKey>> Logger { get; }

        /// <summary>
        /// Gets a service for interacting with an image storage..
        /// </summary>
        protected IImageService ImageService { get; }

        /// <summary>
        /// Gets a repository with images.
        /// </summary>
        protected IEntityRepositoryBase<TKey, TEntity> Repository { get; }

        /// <summary>
        /// Gets limits of images for entity of <c>TEntity</c> type.
        /// </summary>
        protected ImagesLimits<TEntity> Limits { get; }

        /// <inheritdoc/>
        public async Task<Result<string>> UploadImageAsync(TKey entityId, IFormFile image)
        {
            if (image == null)
            {
                return Result<string>.Failed(ImagesOperationErrorCode.NoGivenImagesError.GetOperationError());
            }

            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (entity == null)
            {
                return Result<string>.Failed(ImagesOperationErrorCode.EntityNotFoundError.GetOperationError());
            }

            return await UploadImageProcessAsync(entity, image).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> RemoveImageAsync(TKey entityId, string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.NoGivenImagesError.GetOperationError());
            }

            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (entity == null)
            {
                Logger.LogError($"Entity [id = {entityId}] was not got.");
                return OperationResult.Failed(ImagesOperationErrorCode.EntityNotFoundError.GetOperationError());
            }

            return await RemoveImageProcessAsync(entity, imageId).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ImageUploadingResult> UploadManyImagesAsync(TKey entityId, IList<IFormFile> images)
        {
            if (images == null || images.Count <= 0)
            {
                return new ImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() } };
            }

            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (entity == null)
            {
                Logger.LogError($"Entity [id = {entityId}] was not got.");
                return new ImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.EntityNotFoundError.GetResourceValue() } };
            }

            return await UploadManyImagesProcessAsync(entity, images).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ImageRemovingResult> RemoveManyImagesAsync(TKey entityId, IList<string> imageIds)
        {
            if (imageIds == null || imageIds.Count <= 0)
            {
                return new ImageRemovingResult
                {
                    MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() },
                };
            }

            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (entity == null)
            {
                Logger.LogError($"Entity [id = {entityId}] was not got.");
                return new ImageRemovingResult
                {
                    MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.EntityNotFoundError.GetResourceValue() },
                };
            }

            return await RemoveManyImagesProcessAsync(entity, imageIds).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the entity with included images that is required to be non-null.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <returns>The instance of <see cref="Task"/> that represents <c>TEntity</c>.</returns>
        /// <exception cref="InvalidOperationException">When entity is null.</exception>
        protected async Task<TEntity> GetRequiredEntityWithIncludedImages(TKey entityId)
        {
            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            return entity ?? throw new InvalidOperationException($"Unreal to find {nameof(TEntity)} with id {entityId}.");
        }

        /// <summary>
        /// Gets the entity with included images.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <returns>The instance of <see cref="Task"/> that represents <c>TEntity</c>.</returns>
        /// <exception cref="InvalidOperationException">When filter is not specified.</exception>
        protected virtual async Task<TEntity> GetEntityWithIncludedImages(TKey entityId)
        {
            return (await Repository.GetByFilter(x => x.Id.Equals(entityId), nameof(IImageDependentEntity<TEntity>.Images)).ConfigureAwait(false)).FirstOrDefault();
        }

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
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="OperationResult"/>.</returns>
        private protected async Task<OperationResult> EntityUpdateAsync(TEntity entity)
        {
            _ = entity ?? throw new InvalidOperationException($"Cannot update the {nameof(entity)} because it's null.");
            try
            {
                await Repository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
                return OperationResult.Success;
            }
            catch (DbUpdateException ex)
            {
                Logger.LogError(ex, "Unreal to update entity while uploading images.");
                return OperationResult.Failed(ImagesOperationErrorCode.UpdateEntityError.GetOperationError());
            }
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

            Logger.LogDebug($"Uploading an image for entity [Id = {entity.Id}] was started.");
            if (!AllowedToUploadGivenAmountOfFiles(entity, 1))
            {
                Logger.LogError($"The image limit was reached for the entity [id = {entity.Id}]");
                return Result<string>.Failed(ImagesOperationErrorCode.ExceedingCountOfImagesError.GetOperationError());
            }

            var imageUploadingResult = await ImageService.UploadImageAsync<TEntity>(image).ConfigureAwait(false);
            if (!imageUploadingResult.Succeeded)
            {
                return Result<string>.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError());
            }

            AddImageToEntity(entity, new Image<TEntity> { ExternalStorageId = imageUploadingResult.Value });

            var updatingResult = await EntityUpdateAsync(entity).ConfigureAwait(false);

            if (!updatingResult.Succeeded)
            {
                return Result<string>.Failed(ImagesOperationErrorCode.UpdateEntityError.GetOperationError());
            }

            Logger.LogDebug($"Uploading an image for entity [Id = {entity.Id}] was finished.");
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
            _ = imageId ?? throw new ArgumentNullException(nameof(imageId));

            Logger.LogDebug($"Removing an image [id = {imageId}] for entity [Id = {entity.Id}] was started.");
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

            Logger.LogDebug($"Removing an image [id = {imageId}] for entity Id = {entity.Id} was finished.");
            return await EntityUpdateAsync(entity).ConfigureAwait(false);
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

            Logger.LogDebug($"Uploading images for the entity [Id = {entity.Id}] was started.");
            if (!AllowedToUploadGivenAmountOfFiles(entity, images.Count))
            {
                Logger.LogError($"The image limit was reached for the entity [id = {entity.Id}]");
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
                imagesUploadingResult.SavedIds.ForEach(id => AddImageToEntity(entity, new Image<TEntity> { ExternalStorageId = id }));

                var updatingResult = await EntityUpdateAsync(entity).ConfigureAwait(false);
                if (!updatingResult.Succeeded)
                {
                    return new ImageUploadingResult
                        { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = updatingResult.Errors.FirstOrDefault()?.Description } };
                }
            }

            Logger.LogDebug($"Uploading images for the entity [Id = {entity.Id}] was finished.");
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

            Logger.LogDebug($"Removing images for entity [Id = {entity.Id}] was started.");
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

                var updatingResult = await EntityUpdateAsync(entity).ConfigureAwait(false);
                if (!updatingResult.Succeeded)
                {
                    return new ImageRemovingResult
                    {
                        MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = updatingResult.Errors.FirstOrDefault()?.Description },
                    };
                }
            }

            Logger.LogDebug($"Removing images for entity [Id = {entity.Id}] was finished.");
            return imagesRemovingResult;
        }

        private static void AddImageToEntity(TEntity entity, Image<TEntity> image)
        {
            entity.Images.Add(image);
        }

        private static void RemoveImageFromEntity(TEntity entity, string imageId)
        {
            entity.Images.RemoveAt(
                entity.Images.FindIndex(i => i.ExternalStorageId == imageId));
        }
    }
}
