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
using OutOfSchool.WebApi.Common.SearchFilters;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Services.Images
{
    // TODO: make synchronization to remove incorrect operations' ids

    /// <summary>
    /// Represents a base class for operations with images.
    /// </summary>
    /// <typeparam name="TRepository">Repository type.</typeparam>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TKey">The type of entity Id.</typeparam>
    public abstract class ImageInteractionBaseService<TRepository, TEntity, TKey> : IImageInteractionService<TKey>
        where TRepository : IEntityRepositoryBase<TKey, TEntity>, IImageInteractionRepository
        where TEntity : class, new()
    {
        private readonly ILogger<ImageInteractionBaseService<TRepository, TEntity, TKey>> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInteractionBaseService{TRepository, TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="repository">Repository with images.</param>
        /// <param name="limits">Describes limits of images for entities.</param>
        /// <param name="logger">Logger.</param>
        protected ImageInteractionBaseService(
            IImageService imageService,
            TRepository repository,
            ImagesLimits<TEntity> limits,
            ILogger<ImageInteractionBaseService<TRepository, TEntity, TKey>> logger)
        {
            ImageService = imageService;
            Repository = repository;
            Limits = limits;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a service for interacting with an image storage..
        /// </summary>
        protected IImageService ImageService { get; }

        /// <summary>
        /// Gets a repository with images.
        /// </summary>
        protected TRepository Repository { get; }

        /// <summary>
        /// Gets limits of images for entity of <c>TEntity</c> type.
        /// </summary>
        protected ImagesLimits<TEntity> Limits { get; }

        /// <inheritdoc/>
        public virtual async Task<OperationResult> UploadImageAsync(TKey entityId, IFormFile image)
        {
            if (image == null)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError());
            }

            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (entity == null)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.EntityNotFoundError.GetOperationError());
            }

            if (!AllowedToUploadGivenAmountOfFiles(entity, 1))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.ExceedingCountOfImagesError.GetOperationError());
            }

            var imageUploadingResult = await ImageService.UploadImageAsync<Workshop>(image).ConfigureAwait(false);
            if (!imageUploadingResult.Succeeded)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError());
            }

            AddImageToEntity(entity, new Image<TEntity> { ExternalStorageId = imageUploadingResult.Value });

            return await EntityUpdateAsync(entity).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<OperationResult> RemoveImageAsync(TKey entityId, string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (entity == null)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.EntityNotFoundError.GetOperationError());
            }

            var entityImages = GetEntityImages(entity);
            var ableToRemove = entityImages.Select(x => x.ExternalStorageId).Contains(imageId);

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

            return await EntityUpdateAsync(entity).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<MultipleKeyValueOperationResult> UploadManyImagesAsync(TKey entityId, IList<IFormFile> images)
        {
            if (images == null || images.Count <= 0)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() };
            }

            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (entity == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.EntityNotFoundError.GetResourceValue() };
            }

            return await UploadManyImagesProcessAsync(entity, images).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<MultipleKeyValueOperationResult> RemoveManyImagesAsync(TKey entityId, IList<string> imageIds)
        {
            if (imageIds == null || imageIds.Count <= 0)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() };
            }

            var workshop = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.EntityNotFoundError.GetResourceValue() };
            }

            return await RemoveManyImagesProcessAsync(workshop, imageIds).ConfigureAwait(false);
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
            var filter = GetFilterForSearchingEntityByIdWithIncludedImages(entityId) ?? throw new InvalidOperationException($"Filter for {nameof(TEntity)} is null.");
            return (await Repository.GetByFilter(filter.Predicate, filter.IncludedProperties).ConfigureAwait(false)).FirstOrDefault();
        }

        /// <summary>
        /// Determines if the given count of images is allowed for this entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="countOfFiles">Count of files.</param>
        /// <returns>The <see cref="bool"/> value which shows if uploading is allowed.</returns>
        protected virtual bool AllowedToUploadGivenAmountOfFiles(TEntity entity, int countOfFiles)
        {
            return GetEntityImages(entity).Count + countOfFiles <= Limits.MaxCountOfFiles;
        }

        /// <summary>
        /// Specifies the filter for getting entity with images by id from a repository.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <returns>The instance of <see cref="EntitySearchFilter{TEntity}"/>.</returns>
        protected abstract EntitySearchFilter<TEntity> GetFilterForSearchingEntityByIdWithIncludedImages(TKey entityId);

        /// <summary>
        /// Returns images list for the given entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>The <see cref="List{T}"/> that contains <see cref="Image{TEntity}"/>.</returns>
        protected abstract List<Image<TEntity>> GetEntityImages(TEntity entity);

        /// <summary>
        /// The process of uploading images for the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="images">Images for uploading.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
        protected async Task<MultipleKeyValueOperationResult> UploadManyImagesProcessAsync(
            TEntity entity,
            IList<IFormFile> images)
        {
            if (!AllowedToUploadGivenAmountOfFiles(entity, images.Count))
            {
                return new MultipleKeyValueOperationResult
                    { GeneralResultMessage = ImagesOperationErrorCode.ExceedingCountOfImagesError.GetResourceValue() };
            }

            var imagesUploadingResult = await ImageService.UploadManyImagesAsync<Workshop>(images).ConfigureAwait(false);
            if (imagesUploadingResult.SavedIds == null || imagesUploadingResult.MultipleKeyValueOperationResult == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.UploadingError.GetResourceValue() };
            }

            if (imagesUploadingResult.SavedIds.Count > 0)
            {
                imagesUploadingResult.SavedIds.ForEach(id => AddImageToEntity(entity, new Image<TEntity> { ExternalStorageId = id }));

                var updatingResult = await EntityUpdateAsync(entity).ConfigureAwait(false);
                if (!updatingResult.Succeeded)
                {
                    return new MultipleKeyValueOperationResult { GeneralResultMessage = updatingResult.Errors.FirstOrDefault()?.Description };
                }
            }

            return imagesUploadingResult.MultipleKeyValueOperationResult;
        }

        /// <summary>
        /// The process of removing images for the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageIds">Image ids for removing.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
        protected async Task<MultipleKeyValueOperationResult> RemoveManyImagesProcessAsync(
            TEntity entity,
            IList<string> imageIds)
        {
            var ableToRemove = !imageIds.Except(GetEntityImages(entity).Select(x => x.ExternalStorageId)).Any();

            if (!ableToRemove)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.RemovingError.GetResourceValue() };
            }

            var imagesRemovingResult = await ImageService.RemoveManyImagesAsync(imageIds).ConfigureAwait(false);

            if (imagesRemovingResult.RemovedIds == null || imagesRemovingResult.MultipleKeyValueOperationResult == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.RemovingError.GetResourceValue() };
            }

            if (imagesRemovingResult.RemovedIds.Count > 0)
            {
                imagesRemovingResult.RemovedIds.ForEach(x => RemoveImageFromEntity(entity, x));

                var updatingResult = await EntityUpdateAsync(entity).ConfigureAwait(false);
                if (!updatingResult.Succeeded)
                {
                    return new MultipleKeyValueOperationResult { GeneralResultMessage = updatingResult.Errors.FirstOrDefault()?.Description };
                }
            }

            return imagesRemovingResult.MultipleKeyValueOperationResult;
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="OperationResult"/>.</returns>
        protected async Task<OperationResult> EntityUpdateAsync(TEntity entity)
        {
            try
            {
                await Repository.Update(entity).ConfigureAwait(false);
                return OperationResult.Success;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Unreal to update entity while uploading images.");
                return OperationResult.Failed(ImagesOperationErrorCode.UpdateEntityError.GetOperationError());
            }
        }

        private void AddImageToEntity(TEntity entity, Image<TEntity> image)
        {
            GetEntityImages(entity).Add(image);
        }

        private void RemoveImageFromEntity(TEntity entity, string imageId)
        {
            var entityImages = GetEntityImages(entity);
            entityImages.RemoveAt(
                entityImages.FindIndex(i => i.ExternalStorageId == imageId));
        }
    }
}
