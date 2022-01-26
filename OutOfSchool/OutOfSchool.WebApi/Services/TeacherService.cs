using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources.Codes;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.CustomResults;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Teachers;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Teacher entity.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly ISensitiveEntityRepository<Teacher> teacherRepository;
        private readonly IImageService imageService;
        private readonly ILogger<TeacherService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="teacherRepository">Repository for Teacher entity.</param>
        /// <param name="imageService">Image service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public TeacherService(ISensitiveEntityRepository<Teacher> teacherRepository, IImageService imageService, ILogger<TeacherService> logger, IStringLocalizer<SharedResource> localizer, IMapper mapper)
        {
            this.localizer = localizer;
            this.teacherRepository = teacherRepository;
            this.imageService = imageService;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<CreationResultWithImageDto<TeacherDTO>> Create(TeacherCreationDto dto)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));
            logger.LogInformation("Teacher creating was started.");

            var teacher = mapper.Map<Teacher>(dto);

            var newTeacher = await teacherRepository.Create(teacher).ConfigureAwait(false);

            var uploadingResult = await imageService.UploadImageAsync<Teacher>(dto.ImageFile).ConfigureAwait(false);
            if (uploadingResult.Succeeded)
            {
                await UpdateTeacherAvatarAsync(newTeacher, uploadingResult.Value).ConfigureAwait(false);
            }

            logger.LogInformation($"Teacher with Id = {newTeacher.Id} created successfully.");

            return new CreationResultWithImageDto<TeacherDTO>()
            {
                Dto = mapper.Map<TeacherDTO>(newTeacher),
                UploadingImageResult = uploadingResult.OperationResult,
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherDTO>> GetAll()
        {
            logger.LogInformation("Getting all Teachers started.");

            var teachers = await teacherRepository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!teachers.Any()
                ? "Teacher table is empty."
                : $"All {teachers.Count()} records were successfully received from the Teacher table");

            return teachers.Select(teacher => teacher.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> GetById(Guid id)
        {
            logger.LogInformation($"Getting Teacher by Id started. Looking Id = {id}.");

            var teacher = await teacherRepository.GetById(id).ConfigureAwait(false);

            logger.LogInformation($"Got a Teacher with Id = {id}.");

            return teacher?.ToModel();
        }

        /// <inheritdoc/>
        public async Task<UpdateResultWithImageDto<TeacherDTO>> Update(TeacherUpdateDto dto)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));
            logger.LogInformation($"Updating Teacher with Id = {dto.Id} started.");

            var teacher = await teacherRepository.GetById(dto.Id).ConfigureAwait(false);
            mapper.Map(dto, teacher);

            var changingAvatarResult = await ChangeAvatarImageAsync(teacher, dto.AvatarImageId, dto.ImageFile).ConfigureAwait(false);

            try
            {
                var updatedTeacher = await teacherRepository.Update(teacher).ConfigureAwait(false);

                logger.LogInformation($"Teacher with Id = {updatedTeacher?.Id} updated successfully.");

                return new UpdateResultWithImageDto<TeacherDTO>
                {
                    Dto = mapper.Map<TeacherDTO>(teacher),
                    UploadingImageResult = changingAvatarResult.UploadingResult?.OperationResult,
                    RemovingImageResult = changingAvatarResult.RemovingResult,
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Teacher with Id = {dto.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(Guid id)
        {
            logger.LogInformation($"Deleting Teacher with Id = {id} started.");

            var entity = await teacherRepository.GetByFilterNoTracking(x => x.Id == id, nameof(Workshop.Images)).FirstAsync().ConfigureAwait(false);

            var removingResult = await imageService.RemoveImageAsync(entity.AvatarImageId).ConfigureAwait(false);

            if (!removingResult.Succeeded)
            {
                throw new InvalidOperationException($"Unreal to delete {nameof(Teacher)} [id = {id}] because unable to delete images.");
            }

            try
            {
                await teacherRepository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"Teacher with Id = {id} successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting Teacher with Id = {id} failed.");
                throw;
            }
        }

        private async Task<ImageChangingResult> ChangeAvatarImageAsync(Teacher teacher, string oldImage, IFormFile newImage)
        {
            if (!teacher.AvatarImageId.Equals(oldImage, StringComparison.Ordinal))
            {
                return new ImageChangingResult { RemovingResult = OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError()) };
            }

            var result = new ImageChangingResult();
            if (!string.IsNullOrEmpty(oldImage))
            {
                result.RemovingResult = await imageService.RemoveImageAsync(oldImage).ConfigureAwait(false);
                if (!result.RemovingResult.Succeeded)
                {
                    return new ImageChangingResult { RemovingResult = OperationResult.Failed(ImagesOperationErrorCode.RemovingError.GetOperationError()) };
                }
            }

            result.UploadingResult = await imageService.UploadImageAsync<Teacher>(newImage).ConfigureAwait(false);

            teacher.AvatarImageId = result.UploadingResult.Succeeded ? result.UploadingResult.Value : null;

            return result;
        }

        // delete id if exception
        private async Task UpdateTeacherAvatarAsync(Teacher teacher, string newImageId)
        {
            teacher.AvatarImageId = newImageId;

            try
            {
                await teacherRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Unreal to update entity while operating with images.");
                throw;
            }
        }
    }
}