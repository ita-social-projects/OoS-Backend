using System;
using System.Collections.Generic;
using System.Linq;
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

namespace OutOfSchool.WebApi.Services.Images
{
    // TODO: make synchronization to remove incorrect operations' ids
    public abstract class ImageInteractionBaseService<TRepository, TEntity, TKey> : IImageInteractionService<TKey>
        where TRepository : IEntityRepositoryBase<TKey, TEntity>, IImageInteractionRepository
        where TEntity : class, new()
    {
        private readonly ILogger<ImageInteractionBaseService<TRepository, TEntity, TKey>> logger;

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

        protected IImageService ImageService { get; }

        protected TRepository Repository { get; }

        protected ImagesLimits<TEntity> Limits { get; }

        //protected abstract IList<Image<TEntity>> EntityImages { get; }

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

            var ableToRemove = GetEntityImages(entity).Select(x => x.ExternalStorageId).Contains(imageId);

            if (!ableToRemove)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            var imageRemovingResult = await ImageService.RemoveImageAsync(imageId).ConfigureAwait(false);

            if (!imageRemovingResult.Succeeded)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            GetEntityImages(entity).RemoveAt(GetEntityImages(entity).FindIndex(i => i.ExternalStorageId == imageId));

            return await EntityUpdateAsync(entity).ConfigureAwait(false);
        }

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

        protected async Task<TEntity> GetRequiredEntityWithIncludedImages(TKey entityId)
        {
            var entity = await GetEntityWithIncludedImages(entityId).ConfigureAwait(false);
            return entity ?? throw new InvalidOperationException($"Unreal to find {nameof(TEntity)} with id {entityId}.");
        }

        protected virtual bool AllowedToUploadGivenAmountOfFiles(TEntity entity, int countOfFiles)
        {
            return GetEntityImages(entity).Count + countOfFiles <= Limits.MaxCountOfFiles;
        }

        protected abstract Task<TEntity> GetEntityWithIncludedImages(TKey entityId);

        protected abstract List<Image<TEntity>> GetEntityImages(TEntity entity);

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
            GetEntityImages(entity).RemoveAt(
                GetEntityImages(entity).FindIndex(i => i.ExternalStorageId == imageId));
        }
    }
}
