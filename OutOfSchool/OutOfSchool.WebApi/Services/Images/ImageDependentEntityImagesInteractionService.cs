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

namespace OutOfSchool.WebApi.Services.Images;
// TODO: make synchronization to remove incorrect operations' ids

/// <summary>
/// Represents a mediator class for operations with images.
/// This instance does not save the given entity changes.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
internal class ImageDependentEntityImagesInteractionService<TEntity> : IImageDependentEntityImagesInteractionService<TEntity>
    where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageDependentEntityImagesInteractionService{TEntity}"/> class.
    /// </summary>
    /// <param name="imageService">Service for interacting with an image storage.</param>
    /// <param name="limits">Describes limits of images for entities.</param>
    /// <param name="logger">Logger.</param>
    public ImageDependentEntityImagesInteractionService(
        IImageService imageService,
        IOptions<ImagesLimits<TEntity>> limits,
        ILogger<ImageDependentEntityImagesInteractionService<TEntity>> logger)
    {
        ImageService = imageService;
        Limits = limits.Value;
        Logger = logger;
    }

    /// <summary>
    /// Gets a logger.
    /// </summary>
    protected ILogger<ImageDependentEntityImagesInteractionService<TEntity>> Logger { get; }

    /// <summary>
    /// Gets a service for interacting with an image storage..
    /// </summary>
    protected IImageService ImageService { get; }

    /// <summary>
    /// Gets limits of images for entity of <c>TEntity</c> type.
    /// </summary>
    protected ImagesLimits<TEntity> Limits { get; }

    /// <inheritdoc/>
    public async Task<Result<string>> AddImageAsync(TEntity entity, IFormFile image)
        => await UploadImageProcessAsync(entity, image).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<OperationResult> RemoveImageAsync(TEntity entity, string imageId)
        => await RemoveImageProcessAsync(entity, imageId).ConfigureAwait(false);

    public async Task<ImageChangingResult> ChangeImageAsync(TEntity entity, string oldImageId, IFormFile newImage)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        try
        {
            Logger.LogTrace("Changing an image for the entity was started");

            var result = new ImageChangingResult();

            if (!string.IsNullOrEmpty(oldImageId))
            {
                result.RemovingResult = await RemoveImageProcessAsync(entity, oldImageId).ConfigureAwait(false);
                if (!result.RemovingResult.Succeeded)
                {
                    return result;
                }
            }

            if (newImage != null)
            {
                result.UploadingResult = await UploadImageProcessAsync(entity, newImage).ConfigureAwait(false);
            }

            return result;
        }
        finally
        {
            Logger.LogTrace("Changing an image for the entity was finished");
        }
    }

    /// <inheritdoc/>
    public async Task<MultipleImageUploadingResult> AddManyImagesAsync(TEntity entity, IList<IFormFile> images)
        => await UploadManyImagesProcessAsync(entity, images).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<MultipleImageRemovingResult> RemoveManyImagesAsync(TEntity entity, IList<string> imageIds)
        => await RemoveManyImagesProcessAsync(entity, imageIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<MultipleImageChangingResult> ChangeImagesAsync(TEntity entity, IList<string> oldImageIds, IList<IFormFile> newImages)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));
        _ = entity.Images ?? throw new NullReferenceException($"Entity {nameof(entity.Images)} cannot be null collection");
        oldImageIds ??= new List<string>();

        Logger.LogTrace("Changing images for the entity was started");

        var result = new MultipleImageChangingResult();

        var removingList = entity.Images.Select(x => x.ExternalStorageId).Except(oldImageIds).ToList();

        if (removingList.Any())
        {
            result.RemovedMultipleResult = await RemoveManyImagesProcessAsync(entity, removingList).ConfigureAwait(false);
        }

        if (newImages?.Count > 0)
        {
            result.UploadedMultipleResult = await UploadManyImagesProcessAsync(entity, newImages).ConfigureAwait(false);
        }

        Logger.LogTrace("Changing images for the entity was finished");
        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<string>> AddCoverImageAsync(TEntity entity, IFormFile image)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));
        _ = image ?? throw new ArgumentNullException(nameof(image));

        try
        {
            Logger.LogTrace("Adding a cover image for the entity was started");

            if (!string.IsNullOrEmpty(entity.CoverImageId))
            {
                return Result<string>.Failed(ImagesOperationErrorCode.ImageAlreadyExist.GetOperationError());
            }

            var uploadingCoverImageResult = await ImageService.UploadImageAsync<TEntity>(image).ConfigureAwait(false);

            if (uploadingCoverImageResult.Succeeded)
            {
                entity.CoverImageId = uploadingCoverImageResult.Value;
            }

            return uploadingCoverImageResult;
        }
        finally
        {
            Logger.LogTrace("Adding a cover image for the entity was finished");
        }
    }

    /// <inheritdoc/>
    public async Task<OperationResult> RemoveCoverImageAsync(TEntity entity)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));
        if (string.IsNullOrEmpty(entity.CoverImageId))
        {
            throw new ArgumentException(@"Image id must be a non empty string", nameof(entity));
        }

        Logger.LogTrace("Removing a cover image for the entity was started");
        await ImageService.RemoveImageAsync(entity.CoverImageId).ConfigureAwait(false);

        entity.CoverImageId = null;

        Logger.LogTrace("Removing a cover image for the entity was finished");
        return OperationResult.Success;
    }

    /// <inheritdoc/>
    public async Task<ImageChangingResult> ChangeCoverImageAsync(TEntity entity, string dtoImageId, IFormFile newImage)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        if ((!string.IsNullOrEmpty(dtoImageId) || newImage == null) &&
            string.Equals(dtoImageId, entity.CoverImageId, StringComparison.Ordinal))
        {
            return new ImageChangingResult(); // whenever no need to change cover image
        }

        return await ChangeCoverImageProcessAsync(entity, newImage).ConfigureAwait(false);
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

        try
        {
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

            return imageUploadingResult;
        }
        finally
        {
            Logger.LogTrace("Uploading an image for the entity was finished");
        }
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

        try
        {
            Logger.LogTrace("Removing an image for the entity was started");
            var ableToRemove = entity.Images.Select(x => x.ExternalStorageId).Contains(imageId);

            if (!ableToRemove)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            await ImageService.RemoveImageAsync(imageId).ConfigureAwait(false);

            RemoveImageFromEntity(entity, imageId);

            return OperationResult.Success;
        }
        finally
        {
            Logger.LogTrace("Removing an image for the entity was finished");
        }
    }

    /// <summary>
    /// The process of uploading images for the entity.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="images">Images for uploading.</param>
    /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
    /// <exception cref="ArgumentNullException">When an entity is null.</exception>
    /// <exception cref="ArgumentException">When a given list of images is null or empty.</exception>
    private protected async Task<MultipleImageUploadingResult> UploadManyImagesProcessAsync(
        TEntity entity,
        IList<IFormFile> images)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        if (images == null || images.Count <= 0)
        {
            throw new ArgumentException(@"Given images must be non-null and not empty.", nameof(images));
        }

        try
        {
            Logger.LogTrace("Uploading images for the entity was started");
            if (!AllowedToUploadGivenAmountOfFiles(entity, images.Count))
            {
                Logger.LogTrace("The image limit was reached for the entity");
                return new MultipleImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.ExceedingCountOfImagesError.GetResourceValue() } };
            }

            var imagesUploadingResult = await ImageService.UploadManyImagesAsync<TEntity>(images).ConfigureAwait(false);
            if (imagesUploadingResult.SavedIds == null || imagesUploadingResult.MultipleKeyValueOperationResult == null)
            {
                return new MultipleImageUploadingResult
                    { MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.UploadingError.GetResourceValue() } };
            }

            if (imagesUploadingResult.SavedIds.Count > 0)
            {
                entity.Images.AddRange(imagesUploadingResult.SavedIds.Select(id => new Image<TEntity> { ExternalStorageId = id }));
            }

            return imagesUploadingResult;
        }
        finally
        {
            Logger.LogTrace("Uploading images for the entity was finished");
        }
    }

    /// <summary>
    /// The process of removing images from the entity.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="imageIds">Image ids for removing.</param>
    /// <returns>The instance of <see cref="Task{TResult}"/>, containing <see cref="MultipleKeyValueOperationResult"/>.</returns>
    private protected async Task<MultipleImageRemovingResult> RemoveManyImagesProcessAsync(
        TEntity entity,
        IList<string> imageIds)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        if (imageIds == null || imageIds.Count <= 0)
        {
            throw new ArgumentException(@"Given images must be non-null and not empty.", nameof(imageIds));
        }

        try
        {
            Logger.LogTrace("Removing images for the entity was started");
            var ableToRemove = !imageIds.Except(entity.Images.Select(x => x.ExternalStorageId)).Any();

            if (!ableToRemove)
            {
                return new MultipleImageRemovingResult
                {
                    MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.RemovingError.GetResourceValue() },
                };
            }

            await ImageService.RemoveManyImagesAsync(imageIds).ConfigureAwait(false);

            var imagesRemovingResult = new MultipleImageRemovingResult
            {
                RemovedIds = new List<string>(),
                MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult(),
            };
            for (short i = 0; i < imageIds.Count; i++)
            {
                RemoveImageFromEntity(entity, imageIds[i]);
                imagesRemovingResult.RemovedIds.Add(imageIds[i]);
                imagesRemovingResult.MultipleKeyValueOperationResult.Results.Add(i, OperationResult.Success);
            }

            return imagesRemovingResult;
        }
        finally
        {
            Logger.LogTrace("Removing images for the entity was finished");
        }
    }

    private static void RemoveImageFromEntity(TEntity entity, string imageId)
    {// try to use except
        entity.Images.RemoveAt(
            entity.Images.FindIndex(i => i.ExternalStorageId == imageId));
    }

    private async Task<ImageChangingResult> ChangeCoverImageProcessAsync(TEntity entity, IFormFile newImage)
    {
        try
        {
            Logger.LogTrace("Changing a cover image for the entity was started");

            var changingCoverImageResult = new ImageChangingResult();

            if (!string.IsNullOrEmpty(entity.CoverImageId))
            {
                changingCoverImageResult.RemovingResult = await RemoveCoverImageAsync(entity).ConfigureAwait(false);
                if (!changingCoverImageResult.RemovingResult.Succeeded)
                {
                    return changingCoverImageResult;
                }
            }

            if (string.IsNullOrEmpty(entity.CoverImageId) && newImage != null)
            {
                changingCoverImageResult.UploadingResult = await ImageService.UploadImageAsync<TEntity>(newImage).ConfigureAwait(false);
            }

            entity.CoverImageId = changingCoverImageResult.UploadingResult switch
            {
                null when changingCoverImageResult.RemovingResult is { Succeeded: false } => entity.CoverImageId,
                { Succeeded: true } => changingCoverImageResult.UploadingResult.Value,
                _ => null
            };

            return changingCoverImageResult;
        }
        finally
        {
            Logger.LogTrace("Changing a cover image for the entity was finished");
        }
    }
}