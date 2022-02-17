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
using OutOfSchool.WebApi.Util.Transactions;

namespace OutOfSchool.WebApi.Services.Images
{
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
        /// <param name="transactionProcessor">Responsible for making transactions.</param>
        /// <param name="executionStrategyHelper">Helper for making execution strategies.</param>
        public ImageInteractionBaseService(
            IImageService imageService,
            IEntityRepositoryBase<TKey, TEntity> repository,
            ImagesLimits<TEntity> limits,
            ILogger<ImageInteractionBaseService<TEntity, TKey>> logger,
            IDistributedTransactionProcessor transactionProcessor,
            IExecutionStrategyHelper executionStrategyHelper)
        {
            ImageService = imageService;
            Repository = repository;
            Limits = limits;
            Logger = logger;
            TransactionProcessor = transactionProcessor;
            ExecutionStrategyHelper = executionStrategyHelper;
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

        /// <summary>
        /// Gets the processor that is responsible for making transactions.
        /// </summary>
        protected IDistributedTransactionProcessor TransactionProcessor { get; }

        /// <summary>
        /// Gets the helper for making execution strategies.
        /// </summary>
        protected IExecutionStrategyHelper ExecutionStrategyHelper { get; }

        /// <inheritdoc/>
        public async Task<Result<string>> UploadImageAsync(TKey entityId, IFormFile image, bool enabledTransaction = true)
        {
            if (image == null)
            {
                return Result<string>.Failed(ImagesOperationErrorCode.NoGivenImagesError.GetOperationError());
            }

            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            return await UploadImageProcessAsync(entity, image, enabledTransaction).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> RemoveImageAsync(TKey entityId, string imageId, bool enabledTransaction = true)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.NoGivenImagesError.GetOperationError());
            }

            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            return await RemoveImageProcessAsync(entity, imageId, enabledTransaction).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ImageUploadingResult> UploadManyImagesAsync(TKey entityId, IList<IFormFile> images, bool enabledTransaction = true)
        {
            if (images == null || images.Count <= 0)
            {
                return new ImageUploadingResult
                { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() } };
            }

            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            return await UploadManyImagesProcessAsync(entity, images, enabledTransaction).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ImageRemovingResult> RemoveManyImagesAsync(TKey entityId, IList<string> imageIds, bool enabledTransaction = true)
        {
            if (imageIds == null || imageIds.Count <= 0)
            {
                return new ImageRemovingResult
                {
                    MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() },
                };
            }

            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            return await RemoveManyImagesProcessAsync(entity, imageIds, enabledTransaction).ConfigureAwait(false);
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
        private protected async Task EntityUpdateAsync(TEntity entity)
        {
            _ = entity ?? throw new InvalidOperationException($"Cannot update the {nameof(entity)} because it's null.");
            try
            {
                await Repository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                Logger.LogError(ex, "Unreal to update entity while uploading images.");
                throw;
            }
        }

        /// <summary>
        /// The process of uploading an image for the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="image">Image for uploading.</param>
        /// <param name="enabledTransaction">Determines whether transaction is active for this method.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="OperationResult"/>.</returns>
        /// <exception cref="ArgumentNullException">When entity or image id is null.</exception>
        private protected async Task<Result<string>> UploadImageProcessAsync(
            TEntity entity,
            IFormFile image,
            bool enabledTransaction)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            _ = image ?? throw new ArgumentNullException(nameof(image));

            Logger.LogDebug($"Uploading an image for the entity [Id = {entity.Id}] was started.");
            if (!AllowedToUploadGivenAmountOfFiles(entity, 1))
            {
                Logger.LogError($"The image limit was reached for the entity [id = {entity.Id}]");
                return Result<string>.Failed(ImagesOperationErrorCode.ExceedingCountOfImagesError.GetOperationError());
            }

            var imageUploadingResult = await ExecuteOperationWithResultAsync(UploadImageActionAsync, entity, image, enabledTransaction).ConfigureAwait(false);

            Logger.LogDebug($"Uploading an image for the entity Id = {entity.Id} was finished.");
            return imageUploadingResult;
        }

        /// <summary>
        /// The process of removing an image from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageId">Image Id for removing.</param>
        /// <param name="enabledTransaction">Determines whether transaction is active for this method.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="OperationResult"/>.</returns>
        /// <exception cref="ArgumentNullException">When entity or image id is null.</exception>
        private protected async Task<OperationResult> RemoveImageProcessAsync(
            TEntity entity,
            string imageId,
            bool enabledTransaction)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            _ = imageId ?? throw new ArgumentNullException(nameof(imageId));

            Logger.LogDebug($"Removing an image [id = {imageId}] for the entity [Id = {entity.Id}] was started.");

            var ableToRemove = entity.Images.Select(x => x.ExternalStorageId).Contains(imageId);

            if (!ableToRemove)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            var imageRemovingResult = await ExecuteOperationWithResultAsync(RemoveImageActionAsync, entity, imageId, enabledTransaction).ConfigureAwait(false);

            Logger.LogDebug($"Removing an image [id = {imageId}] for the entity Id = {entity.Id} was finished.");
            return imageRemovingResult;
        }

        /// <summary>
        /// The process of uploading images for the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="images">Images for uploading.</param>
        /// <param name="enabledTransaction">Determines whether transaction is active for this method.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
        /// <exception cref="ArgumentNullException">When an entity is null.</exception>
        /// <exception cref="ArgumentException">When a given list of images is null or empty.</exception>
        private protected async Task<ImageUploadingResult> UploadManyImagesProcessAsync(
            TEntity entity,
            IList<IFormFile> images,
            bool enabledTransaction)
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

            var imagesUploadingResult =
                await ExecuteOperationWithResultAsync(UploadManyImagesActionAsync, entity, images, enabledTransaction)
                    .ConfigureAwait(false);

            Logger.LogDebug($"Uploading images for the entity [Id = {entity.Id}] was finished.");
            return imagesUploadingResult;
        }

        /// <summary>
        /// The process of removing images from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageIds">Image ids for removing.</param>
        /// <param name="enabledTransaction">Determines whether transaction is active for this method.</param>
        /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
        private protected async Task<ImageRemovingResult> RemoveManyImagesProcessAsync(
            TEntity entity,
            IList<string> imageIds,
            bool enabledTransaction)
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

            var imagesRemovingResult =
                await ExecuteOperationWithResultAsync(RemoveManyImagesActionAsync, entity, imageIds, enabledTransaction)
                    .ConfigureAwait(false);

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

        private async Task<Result<string>> UploadImageActionAsync(TEntity entity, IFormFile image)
        {
            var imageUploadingResult = await ImageService.UploadImageAsync<TEntity>(image).ConfigureAwait(false);
            if (imageUploadingResult.Succeeded)
            {
                AddImageToEntity(entity, new Image<TEntity> { ExternalStorageId = imageUploadingResult.Value });
                await EntityUpdateAsync(entity).ConfigureAwait(false);
            }

            Logger.LogDebug($"Uploading an image for entity [Id = {entity.Id}] was finished.");
            return imageUploadingResult;
        }

        private async Task<OperationResult> RemoveImageActionAsync(TEntity entity, string imageId)
        {
            var imageRemovingResult = await ImageService.RemoveImageAsync(imageId).ConfigureAwait(false);

            if (imageRemovingResult.Succeeded)
            {
                RemoveImageFromEntity(entity, imageId);
                await EntityUpdateAsync(entity).ConfigureAwait(false);
            }

            return imageRemovingResult;
        }

        private async Task<ImageUploadingResult> UploadManyImagesActionAsync(TEntity entity, IList<IFormFile> images)
        {
            var imagesUploadingResult = await ImageService.UploadManyImagesAsync<TEntity>(images).ConfigureAwait(false);
            if (imagesUploadingResult.SavedIds?.Count > 0)
            {
                imagesUploadingResult.SavedIds.ForEach(id => AddImageToEntity(entity, new Image<TEntity> { ExternalStorageId = id }));
                await EntityUpdateAsync(entity).ConfigureAwait(false);
            }

            return imagesUploadingResult;
        }

        private async Task<ImageRemovingResult> RemoveManyImagesActionAsync(TEntity entity, IList<string> imageIds)
        {
            var imagesRemovingResult = await ImageService.RemoveManyImagesAsync(imageIds).ConfigureAwait(false);
            if (imagesRemovingResult.RemovedIds.Count > 0)
            {
                imagesRemovingResult.RemovedIds.ForEach(x => RemoveImageFromEntity(entity, x));
                await EntityUpdateAsync(entity).ConfigureAwait(false);
            }

            return imagesRemovingResult;
        }

        private async Task<TResult> RunInDefaultTransactionWithResultAsync<T1, T2, TResult>(Func<T1, T2, Task<TResult>> operation, T1 param1, T2 param2)
        {
            var strategy = ExecutionStrategyHelper.CreateStrategyByDbName(DbContextName.OutOfSchoolDbContext);
            return await strategy.ExecuteAsync(async () =>
                await TransactionProcessor.RunTransactionWithAutoCommitOrRollBackAsync(
                    new[] { DbContextName.OutOfSchoolDbContext, DbContextName.FilesDbContext },
                    async () => await operation(param1, param2).ConfigureAwait(false)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task<TResult> ExecuteOperationWithResultAsync<T1, T2, TResult>(Func<T1, T2, Task<TResult>> operation, T1 param1, T2 param2, bool enabledTransaction)
        {
            if (enabledTransaction)
            {
                return await RunInDefaultTransactionWithResultAsync(operation, param1, param2).ConfigureAwait(false);
            }

            return await operation(param1, param2).ConfigureAwait(false);
        }
    }
}
