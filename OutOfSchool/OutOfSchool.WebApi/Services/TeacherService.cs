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
        private readonly ITeacherImagesInteractionService teacherImagesInteractionService;
        private readonly ILogger<TeacherService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="teacherRepository">Repository for Teacher entity.</param>
        /// <param name="teacherImagesInteractionService">Image service for teachers.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public TeacherService(ISensitiveEntityRepository<Teacher> teacherRepository, ITeacherImagesInteractionService teacherImagesInteractionService, ILogger<TeacherService> logger, IStringLocalizer<SharedResource> localizer, IMapper mapper)
        {
            this.localizer = localizer;
            this.teacherRepository = teacherRepository;
            this.teacherImagesInteractionService = teacherImagesInteractionService;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<CreationResultWithImageDto<TeacherDTO>> Create(TeacherCreationDto dto)
        {
            logger.LogInformation("Teacher creating was started.");

            var teacher = mapper.Map<Teacher>(dto);

            var newTeacher = await teacherRepository.Create(teacher).ConfigureAwait(false);

            Result<string> uploadingResult = null;
            if (dto.ImageFile != null)
            {
                uploadingResult = await teacherImagesInteractionService.UploadImageAsync(newTeacher.Id, dto.ImageFile).ConfigureAwait(false);

                if (uploadingResult.Succeeded)
                {
                    await UpdateTeacherAvatarAsync(newTeacher, uploadingResult.Value).ConfigureAwait(false);
                }
            }

            logger.LogInformation($"Teacher with Id = {newTeacher?.Id} created successfully.");

            return new CreationResultWithImageDto<TeacherDTO>()
            {
                Dto = mapper.Map<TeacherDTO>(newTeacher),
                UploadingImageResult = uploadingResult?.OperationResult,
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

            if (teacher == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a Teacher with Id = {id}.");

            return teacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<UpdateResultWithImageDto<TeacherDTO>> Update(TeacherUpdateDto dto)
        {
            logger.LogInformation($"Updating Teacher with Id = {dto.Id} started.");

            var updatingImageResult =
                await teacherImagesInteractionService.UpdateImageAsync(dto.Id, dto.AvatarImageId, dto.ImageFile).ConfigureAwait(false);

            var teacher = await teacherRepository.GetById(dto.Id).ConfigureAwait(false);
            mapper.Map(dto, teacher);

            if (updatingImageResult.UploadingResult is { Succeeded: true })
            {
                teacher.AvatarImageId = updatingImageResult.UploadingResult.Value;
            }

            try
            {
                var updatedTeacher = await teacherRepository.Update(teacher).ConfigureAwait(false);

                logger.LogInformation($"Teacher with Id = {updatedTeacher?.Id} updated successfully.");

                return new UpdateResultWithImageDto<TeacherDTO>
                {
                    Dto = mapper.Map<TeacherDTO>(teacher),
                    UploadingImageResult = updatingImageResult.UploadingResult?.OperationResult,
                    RemovingImageResult = updatingImageResult.RemovingResult,
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

            var removingResult = await teacherImagesInteractionService.RemoveManyImagesAsync(entity.Id, entity.Images.Select(x => x.ExternalStorageId).ToList()).ConfigureAwait(false);

            if (entity.Images.Count > 0 && removingResult.MultipleKeyValueOperationResult is { Succeeded: false })
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

        // TODO: delete image ids when exception
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