using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Codes;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services.Images
{
    // TODO: make synchronization to remove incorrect operations' ids

    /// <summary>
    /// Provides APIs for making operations with <see cref="Workshop"/> images.
    /// </summary>
    public class WorkshopImagesService : IWorkshopImagesService
    {
        private readonly IWorkshopRepository workshopRepository;
        private readonly IImageService imageService;
        private readonly ILogger<WorkshopImagesService> logger;
        private readonly ImagesLimits<Workshop> limits;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopImagesService"/> class.
        /// </summary>
        /// <param name="workshopRepository">Repository for <see cref="Workshop"/> entities.</param>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="limits">Images limits for <see cref="Workshop"/> instances.</param>
        /// <param name="logger">Logger.</param>
        public WorkshopImagesService(IWorkshopRepository workshopRepository, IImageService imageService, IOptions<ImagesLimits<Workshop>> limits, ILogger<WorkshopImagesService> logger)
        {
            this.workshopRepository = workshopRepository;
            this.imageService = imageService;
            this.limits = limits.Value;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<OperationResult> UploadImageAsync(Guid entityId, IFormFile image)
        {
            if (image == null)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError());
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.EntityNotFoundError.GetOperationError());
            }

            if (!AllowedToUploadGivenAmountOfFiles(workshop, 1))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.ExceedingCountOfImagesError.GetOperationError());
            }

            var imageUploadingResult = await imageService.UploadImageAsync<Workshop>(image).ConfigureAwait(false);
            if (!imageUploadingResult.Succeeded)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.UploadingError.GetOperationError());
            }

            workshop.WorkshopImages.Add(new Image<Workshop> { ExternalStorageId = imageUploadingResult.Value });

            return await WorkshopUpdateAsync(workshop).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> RemoveImageAsync(Guid entityId, string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.EntityNotFoundError.GetOperationError());
            }

            var ableToRemove = workshop.WorkshopImages.Select(x => x.ExternalStorageId).Contains(imageId);

            if (!ableToRemove)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            var imageRemovingResult = await imageService.RemoveImageAsync(imageId).ConfigureAwait(false);

            if (!imageRemovingResult.Succeeded)
            {
                return OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError());
            }

            workshop.WorkshopImages.RemoveAt(workshop.WorkshopImages.FindIndex(i => i.ExternalStorageId == imageId));

            return await WorkshopUpdateAsync(workshop).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MultipleKeyValueOperationResult> UploadManyImagesAsync(Guid entityId, List<IFormFile> images)
        {
            if (images == null || images.Count <= 0)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() };
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.EntityNotFoundError.GetResourceValue() };
            }

            return await UploadManyImagesProcessAsync(workshop, images).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<MultipleKeyValueOperationResult> RemoveManyImagesAsync(Guid entityId, List<string> imageIds)
        {
            if (imageIds == null || imageIds.Count <= 0)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.NoGivenImagesError.GetResourceValue() };
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.EntityNotFoundError.GetResourceValue() };
            }

            return await RemoveManyImagesProcessAsync(workshop, imageIds).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ImageChangingResult> ChangeImagesAsync(WorkshopUpdateDto dto)
        {
            ValidateWorkshopUpdateDto(dto);

            var workshop = await GetRequiredWorkshopWithIncludedImages(dto.Id).ConfigureAwait(false);

            var result = new ImageChangingResult();

            var shouldRemove = !new HashSet<string>(dto.ImageIds).SetEquals(workshop.WorkshopImages.Select(x => x.ExternalStorageId));

            if (shouldRemove)
            {
                var removingList = workshop.WorkshopImages.Select(x => x.ExternalStorageId).Except(dto.ImageIds).ToList();
                result.RemovedMultipleResult = await RemoveManyImagesProcessAsync(workshop, removingList).ConfigureAwait(false);
            }

            if (result.RemovedMultipleResult is { HasResults: true })
            {
                workshop = await GetRequiredWorkshopWithIncludedImages(workshop.Id).ConfigureAwait(false);
            }

            if (dto.ImageFiles?.Count > 0)
            {
                result.UploadedMultipleResult = await UploadManyImagesProcessAsync(workshop, dto.ImageFiles).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<OperationResult> WorkshopUpdateAsync(Workshop workshop)
        {
            try
            {
                await workshopRepository.Update(workshop).ConfigureAwait(false);
                return OperationResult.Success;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, $"Unreal to update workshop with id = {workshop.Id}.");
                return OperationResult.Failed(ImagesOperationErrorCode.UpdateEntityError.GetOperationError());
            }
        }

        private async Task<MultipleKeyValueOperationResult> UploadManyImagesProcessAsync(
            Workshop workshop,
            List<IFormFile> images)
        {
            if (!AllowedToUploadGivenAmountOfFiles(workshop, images.Count))
            {
                return new MultipleKeyValueOperationResult
                    { GeneralResultMessage = ImagesOperationErrorCode.ExceedingCountOfImagesError.GetResourceValue() };
            }

            var imagesUploadingResult = await imageService.UploadManyImagesAsync<Workshop>(images).ConfigureAwait(false);
            if (imagesUploadingResult.SavedIds == null || imagesUploadingResult.MultipleKeyValueOperationResult == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.UploadingError.GetResourceValue() };
            }

            if (imagesUploadingResult.SavedIds.Count > 0)
            {
                imagesUploadingResult.SavedIds.ForEach(id => workshop.WorkshopImages.Add(new Image<Workshop> { ExternalStorageId = id }));

                var updatingResult = await WorkshopUpdateAsync(workshop).ConfigureAwait(false);
                if (!updatingResult.Succeeded)
                {
                    return new MultipleKeyValueOperationResult { GeneralResultMessage = updatingResult.Errors.FirstOrDefault()?.Description };
                }
            }

            return imagesUploadingResult.MultipleKeyValueOperationResult;
        }

        private async Task<MultipleKeyValueOperationResult> RemoveManyImagesProcessAsync(
            Workshop workshop,
            List<string> imageIds)
        {
            var ableToRemove = !imageIds.Except(workshop.WorkshopImages.Select(x => x.ExternalStorageId)).Any();

            if (!ableToRemove)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.RemovingError.GetResourceValue() };
            }

            var imagesRemovingResult = await imageService.RemoveManyImagesAsync(imageIds).ConfigureAwait(false);

            if (imagesRemovingResult.RemovedIds == null || imagesRemovingResult.MultipleKeyValueOperationResult == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ImagesOperationErrorCode.RemovingError.GetResourceValue() };
            }

            if (imagesRemovingResult.RemovedIds.Count > 0)
            {
                imagesRemovingResult.RemovedIds.ForEach(x =>
                {
                    workshop.WorkshopImages.RemoveAt(
                        workshop.WorkshopImages.FindIndex(i => i.ExternalStorageId == x));
                });

                var updatingResult = await WorkshopUpdateAsync(workshop).ConfigureAwait(false);
                if (!updatingResult.Succeeded)
                {
                    return new MultipleKeyValueOperationResult { GeneralResultMessage = updatingResult.Errors.FirstOrDefault()?.Description };
                }
            }

            return imagesRemovingResult.MultipleKeyValueOperationResult;
        }

        private void ValidateWorkshopUpdateDto(WorkshopUpdateDto dto)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto)); // TODO: think about another result
            _ = dto.ImageIds ?? throw new InvalidOperationException($"{nameof(dto.ImageIds)} cannot be null.");
        }

        private async Task<Workshop> GetWorkshopWithIncludedImages(Guid entityId)
        {
            return (await workshopRepository.GetByFilter(x => x.Id == entityId, nameof(Workshop.WorkshopImages)).ConfigureAwait(false)).FirstOrDefault();
        }

        private async Task<Workshop> GetRequiredWorkshopWithIncludedImages(Guid entityId)
        {
            return (await workshopRepository.GetByFilter(x => x.Id == entityId, nameof(Workshop.WorkshopImages)).ConfigureAwait(false)).First();
        }

        private bool AllowedToUploadGivenAmountOfFiles(Workshop workshop, int countOfFiles)
        {
            return workshop.WorkshopImages.Count + countOfFiles <= limits.MaxCountOfFiles;
        }
    }
}
