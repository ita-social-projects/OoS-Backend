using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Util.Transactions;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Represents a class for operations with images.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TKey">The type of entity Id.</typeparam>
    public class ChangeableImagesInteractionService<TEntity, TKey> :
        ImageInteractionBaseService<TEntity, TKey>,
        IChangeableImagesInteractionService<TKey>
        where TEntity : class, IKeyedEntity<TKey>, IImageDependentEntity<TEntity>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeableImagesInteractionService{TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="repository">Repository with images.</param>
        /// <param name="limits">Describes limits of images for entities.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="transactionProcessor">Responsible for making transactions.</param>
        /// <param name="executionStrategyHelper">Helper for making execution strategies.</param>
        public ChangeableImagesInteractionService(
            IImageService imageService,
            IEntityRepositoryBase<TKey, TEntity> repository,
            ImagesLimits<TEntity> limits,
            ILogger<ChangeableImagesInteractionService<TEntity, TKey>> logger,
            IDistributedTransactionProcessor transactionProcessor,
            IExecutionStrategyHelper executionStrategyHelper)
            : base(imageService, repository, limits, logger, transactionProcessor, executionStrategyHelper)
        {
        }

        /// <inheritdoc/>
        public async Task<MultipleImageChangingResult> ChangeImagesAsync(TKey entityId, IList<string> oldImageIds, IList<IFormFile> newImages, bool enabledTransaction = true)
        {
            _ = oldImageIds ?? throw new ArgumentNullException(nameof(oldImageIds));

            Logger.LogDebug($"Changing images for entity [Id = {entityId}] was started.");
            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            var result = await ExecuteOperationWithResultAsync(ChangeImagesProcessAsync, entity, oldImageIds, newImages, enabledTransaction).ConfigureAwait(false);
            Logger.LogDebug($"Changing images for entity [Id = {entityId}] was finished.");
            return result;
        }

        /// <inheritdoc/>
        public async Task<ImageChangingResult> UpdateImageAsync(TKey entityId, string oldImageId, IFormFile newImage, bool enabledTransaction = true)
        {
            Logger.LogDebug($"Updating an image for entity [Id = {entityId}] was started.");
            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            var result = await ExecuteOperationWithResultAsync(UpdateImageProcessAsync, entity, oldImageId, newImage, enabledTransaction).ConfigureAwait(false);
            Logger.LogDebug($"Updating an image for entity [Id = {entityId}] was finished.");
            return result;
        }

        private async Task<MultipleImageChangingResult> ChangeImagesProcessAsync(TEntity entity, IList<string> oldImageIds, IList<IFormFile> newImages)
        {
            var result = new MultipleImageChangingResult();

            var shouldRemove = !new HashSet<string>(oldImageIds).SetEquals(entity.Images.Select(x => x.ExternalStorageId));

            if (shouldRemove)
            {
                var removingList = entity.Images.Select(x => x.ExternalStorageId).Except(oldImageIds).ToList();
                if (removingList.Any())
                {
                    result.RemovedMultipleResult = await RemoveManyImagesProcessAsync(entity, removingList, false).ConfigureAwait(false);
                }
            }

            if (newImages?.Count > 0)
            {
                result.UploadedMultipleResult = await UploadManyImagesProcessAsync(entity, newImages, false).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<ImageChangingResult> UpdateImageProcessAsync(TEntity entity, string oldImageId, IFormFile newImage)
        {
            var result = new ImageChangingResult();

            if (!string.IsNullOrEmpty(oldImageId))
            {
                result.RemovingResult = await RemoveImageProcessAsync(entity, oldImageId, false).ConfigureAwait(false);
            }

            if (!result.RemovingResult.Succeeded)
            {
                return result;
            }

            if (newImage != null)
            {
                result.UploadingResult = await UploadImageProcessAsync(entity, newImage, false).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<TResult> RunInDefaultTransactionWithResultAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> operation, T1 param1, T2 param2, T3 param3)
        {
            var strategy = ExecutionStrategyHelper.CreateStrategyByDbName(DbContextName.OutOfSchoolDbContext);
            return await strategy.ExecuteAsync(async () =>
                await TransactionProcessor.RunTransactionWithAutoCommitOrRollBackAsync(
                    new[] { DbContextName.OutOfSchoolDbContext, DbContextName.FilesDbContext },
                    async () => await operation(param1, param2, param3).ConfigureAwait(false)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task<TResult> ExecuteOperationWithResultAsync<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> operation, T1 param1, T2 param2, T3 param3, bool enabledTransaction)
        {
            if (enabledTransaction)
            {
                return await RunInDefaultTransactionWithResultAsync(operation, param1, param2, param3).ConfigureAwait(false);
            }

            return await operation(param1, param2, param3).ConfigureAwait(false);
        }
    }
}
