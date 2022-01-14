﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public abstract class ChangeableImagesInteractionService<TRepository, TEntity, TKey> :
        ImageInteractionBaseService<TRepository, TEntity, TKey>,
        IChangeableImagesInteractionService<TKey>
        where TRepository : IEntityRepositoryBase<TKey, TEntity>, IImageInteractionRepository
        where TEntity : class, new()
    {
        protected ChangeableImagesInteractionService(
            IImageService imageService,
            TRepository repository,
            ILogger<ImageInteractionBaseService<TRepository, TEntity, TKey>> logger,
            ImagesLimits<TEntity> limits)
            : base(imageService, repository, limits, logger)
        {
        }

        //protected abstract override IList<Image<TEntity>> EntityImages { get; }

        public virtual async Task<ImageChangingResult> ChangeImagesAsync(TKey entityId, IList<string> oldImageIds, IList<IFormFile> newImages)
        {
            _ = oldImageIds ?? throw new ArgumentNullException(nameof(oldImageIds));

            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            var result = new ImageChangingResult();

            var shouldRemove = !new HashSet<string>(oldImageIds).SetEquals(GetEntityImages(entity).Select(x => x.ExternalStorageId));

            if (shouldRemove)
            {
                var removingList = GetEntityImages(entity).Select(x => x.ExternalStorageId).Except(oldImageIds).ToList();
                result.RemovedMultipleResult = await RemoveManyImagesProcessAsync(entity, removingList).ConfigureAwait(false);
            }

            if (result.RemovedMultipleResult is { HasResults: true })
            {
                entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);
            }

            if (newImages?.Count > 0)
            {
                result.UploadedMultipleResult = await UploadManyImagesProcessAsync(entity, newImages).ConfigureAwait(false);
            }

            return result;
        }

        protected abstract override Task<TEntity> GetEntityWithIncludedImages(TKey entityId);

        protected abstract override List<Image<TEntity>> GetEntityImages(TEntity entity);
    }
}
