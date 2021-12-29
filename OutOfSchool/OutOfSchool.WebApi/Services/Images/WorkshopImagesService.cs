using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Describers;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services.Images
{
    public class WorkshopImagesService : IWorkshopImagesService
    {
        private readonly IWorkshopRepository workshopRepository;
        private readonly IImageService imageService;
        private readonly ILogger<WorkshopImagesService> logger;

        public WorkshopImagesService(IWorkshopRepository workshopRepository, IImageService imageService, ILogger<WorkshopImagesService> logger)
        {
            this.workshopRepository = workshopRepository;
            this.imageService = imageService;
            this.logger = logger;
            ErrorDescriber = imageService.ErrorDescriber;
        }

        public ImagesErrorDescriber ErrorDescriber { get; }

        // TODO: check the workshop's images limit in order to prevent uploading too many images into 1 workshop
        public async Task<OperationResult> UploadImageAsync(Guid entityId, IFormFile image)
        {
            if (image == null)
            {
                return OperationResult.Failed(ErrorDescriber.UploadingError());
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return OperationResult.Failed(ErrorDescriber.EntityNotFoundError());
            }

            var imageUploadingResult = await imageService.UploadImageAsync<Workshop>(image).ConfigureAwait(false);
            if (!imageUploadingResult.Succeeded)
            {
                return OperationResult.Failed(ErrorDescriber.UploadingError());
            }

            workshop.WorkshopImages.Add(new Image<Workshop> { ExternalStorageId = imageUploadingResult.Value });

            return await WorkshopUpdateAsync(workshop).ConfigureAwait(false);
        }

        public async Task<OperationResult> RemoveImageAsync(Guid entityId, string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                return OperationResult.Failed(ErrorDescriber.RemovingError());
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return OperationResult.Failed(ErrorDescriber.EntityNotFoundError());
            }

            var ableToRemove = workshop.WorkshopImages.Select(x => x.ExternalStorageId).Contains(imageId);

            if (!ableToRemove)
            {
                return OperationResult.Failed(ErrorDescriber.RemovingError());
            }

            var imageRemovingResult = await imageService.RemoveImageAsync(imageId).ConfigureAwait(false);

            if (!imageRemovingResult.Succeeded)
            {
                return OperationResult.Failed(ErrorDescriber.RemovingError());
            }

            workshop.WorkshopImages.RemoveAt(workshop.WorkshopImages.FindIndex(i => i.ExternalStorageId == imageId));

            return await WorkshopUpdateAsync(workshop).ConfigureAwait(false);
        }

        // TODO: check the workshop's images limit in order to prevent uploading too many images into 1 workshop
        public async Task<MultipleKeyValueOperationResult> UploadManyImagesAsync(Guid entityId, List<IFormFile> images)
        {
            if (images == null || images.Count <= 0)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.NoGivenImagesError };
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.EntityNotFoundError };
            }

            var imagesUploadingResult = await imageService.UploadManyImagesAsync<Workshop>(images).ConfigureAwait(false);
            if (imagesUploadingResult.SavedIds == null || imagesUploadingResult.MultipleKeyValueOperationResult == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.UploadingError };
            }

            if (imagesUploadingResult.SavedIds.Count > 0)
            {
                imagesUploadingResult.SavedIds.ForEach(id => workshop.WorkshopImages.Add(new Image<Workshop> { ExternalStorageId = id }));

                if (!(await WorkshopUpdateAsync(workshop).ConfigureAwait(false)).Succeeded)
                { // TODO: change response
                    return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.UploadingError };
                }
            }

            return imagesUploadingResult.MultipleKeyValueOperationResult;
        }

        public async Task<MultipleKeyValueOperationResult> RemoveManyImagesAsync(Guid entityId, List<string> imageIds)
        {
            if (imageIds == null || imageIds.Count <= 0)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.NoGivenImagesError };
            }

            var workshop = await GetWorkshopWithIncludedImages(entityId).ConfigureAwait(false);
            if (workshop == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.EntityNotFoundError };
            }

            var ableToRemove = !imageIds.Except(workshop.WorkshopImages.Select(x => x.ExternalStorageId)).Any();

            if (!ableToRemove)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.RemovingError };
            }

            var imagesRemovingResult = await imageService.RemoveManyImagesAsync(imageIds).ConfigureAwait(false);

            if (imagesRemovingResult.RemovedIds == null || imagesRemovingResult.MultipleKeyValueOperationResult == null)
            {
                return new MultipleKeyValueOperationResult { GeneralResultMessage = ResourceInstances.ImageResource.UploadingError };
            }

            if (imagesRemovingResult.RemovedIds.Count > 0)
            {
                imagesRemovingResult.RemovedIds.ForEach(x =>
                {
                    workshop.WorkshopImages.RemoveAt(
                        workshop.WorkshopImages.FindIndex(i => i.ExternalStorageId == x));
                });

                if (!(await WorkshopUpdateAsync(workshop).ConfigureAwait(false)).Succeeded)
                {
                    // TODO: change response
                    return new MultipleKeyValueOperationResult
                    { GeneralResultMessage = ResourceInstances.ImageResource.UploadingError };
                }
            }

            return imagesRemovingResult.MultipleKeyValueOperationResult;
        }

        public async Task<ImageChangingResult> ChangeImagesAsync(WorkshopUpdateDto dto)
        {
            ValidateWorkshopUpdateDto(dto);

            var workshop = await GetWorkshopWithIncludedImages(dto.Id).ConfigureAwait(false);
            if (workshop == null)
            {
                throw new InvalidOperationException($"Workshop with id = {dto.Id} not found.");
            }

            var result = new ImageChangingResult();

            var shouldRemove = !new HashSet<string>(dto.ImageIds).SetEquals(workshop.WorkshopImages.Select(x => x.ExternalStorageId));

            if (shouldRemove)
            {
                var removingList = workshop.WorkshopImages.Select(x => x.ExternalStorageId).Except(dto.ImageIds).ToList();
                result.RemovedMultipleResult = await RemoveManyImagesAsync(dto.Id, removingList).ConfigureAwait(false);
            }

            if (dto.ImageFiles.Count > 0)
            {
                result.UploadedMultipleResult =
                    await UploadManyImagesAsync(dto.Id, dto.ImageFiles).ConfigureAwait(false);
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
                return OperationResult.Failed(ErrorDescriber.UpdateEntityError());
            }
        }

        private void ValidateWorkshopUpdateDto(WorkshopUpdateDto dto)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto)); // TODO: think about another result
            _ = dto.ImageIds ?? throw new InvalidOperationException($"{nameof(dto.ImageIds)} cannot be null.");
            _ = dto.ImageFiles ?? throw new InvalidOperationException($"{nameof(dto.ImageFiles)} cannot be null.");
        }

        private async Task<Workshop> GetWorkshopWithIncludedImages(Guid entityId)
        {
            return (await workshopRepository.GetByFilter(x => x.Id == entityId, nameof(Workshop.WorkshopImages)).ConfigureAwait(false)).FirstOrDefault();
        }
    }
}
