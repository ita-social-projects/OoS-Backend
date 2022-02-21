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
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Teachers;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util.Transactions;

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
        private readonly IDistributedTransactionProcessor transactionProcessor;
        private readonly IExecutionStrategyHelper executionStrategyHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="teacherRepository">Repository for Teacher entity.</param>
        /// <param name="imageService">Image service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public TeacherService(
            ISensitiveEntityRepository<Teacher> teacherRepository,
            IImageService imageService,
            ILogger<TeacherService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            IDistributedTransactionProcessor transactionProcessor,
            IExecutionStrategyHelper executionStrategyHelper)
        {
            this.localizer = localizer;
            this.teacherRepository = teacherRepository;
            this.imageService = imageService;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionProcessor = transactionProcessor;
            this.executionStrategyHelper = executionStrategyHelper;
        }

        /// <inheritdoc/>
        public async Task<TeacherCreationResultDto> Create(TeacherCreationDto dto, bool enabledTransaction = true)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));

            return await ExecuteOperationWithResultAsync(CreateProcessAsync, dto, enabledTransaction).ConfigureAwait(false);
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

            var teacher = (await teacherRepository.GetByFilter(t => t.Id == id).ConfigureAwait(false)).SingleOrDefault();

            if (teacher == null)
            {
                throw new ArgumentException(
                    nameof(id),
                    paramName: $"There are no recors in teachers table with such id - {id}.");
            }

            logger.LogInformation($"Got a Teacher with Id = {id}.");

            return teacher?.ToModel();
        }

        /// <inheritdoc/>
        public async Task<TeacherUpdateResultDto> Update(TeacherUpdateDto dto, bool enabledTransaction = true)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));

            return await ExecuteOperationWithResultAsync(UpdateProcessAsync, dto, enabledTransaction).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task Delete(Guid id, bool enabledTransaction = true)
        {
            await ExecuteOperationAsync(DeleteProcessAsync, id, enabledTransaction).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Guid> GetTeachersWorkshopId(Guid teacherId)
        {
            logger.LogInformation($"Searching Teacher by Id started. Looking Id = {teacherId}.");

            var teacher = await teacherRepository.GetByFilterNoTracking(t => t.Id == teacherId).SingleOrDefaultAsync().ConfigureAwait(false);

            if (teacher == null)
            {
                throw new ArgumentException($"There are no recors in teachers table with such id - {teacherId}.", nameof(teacherId));
            }

            logger.LogInformation($"Successfully found a Teacher with Id = {teacherId}.");
            var teachersWorkshopId = teacher.WorkshopId;
            logger.LogInformation($"Successfully found WorkshopId - {teachersWorkshopId} for Teacher  with Id = {teacherId}.");
            return teachersWorkshopId;
        }

        private async Task<TeacherCreationResultDto> CreateProcessAsync(TeacherCreationDto dto)
        {
            logger.LogInformation("Teacher creating was started.");
            var teacher = mapper.Map<Teacher>(dto);

            var newTeacher = await teacherRepository.Create(teacher).ConfigureAwait(false);

            var uploadingResult = await imageService.UploadImageAsync<Teacher>(dto.AvatarImage).ConfigureAwait(false);
            if (uploadingResult.Succeeded)
            {
                teacher.AvatarImageId = uploadingResult.Value;
                await UpdateTeacher().ConfigureAwait(false);
            }

            logger.LogInformation($"Teacher with Id = {newTeacher.Id} created successfully.");

            return new TeacherCreationResultDto
            {
                Teacher = mapper.Map<TeacherDTO>(newTeacher),
                UploadingAvatarImageResult = uploadingResult.OperationResult,
            };
        }

        private async Task<TeacherUpdateResultDto> UpdateProcessAsync(TeacherUpdateDto dto)
        {
            logger.LogInformation($"Updating Teacher with Id = {dto.Id} started.");

            var teacher = await teacherRepository.GetById(dto.Id).ConfigureAwait(false);

            mapper.Map(dto, teacher);

            var changingAvatarResult = await ChangeAvatarImage(teacher, dto.AvatarImageId, dto.AvatarImage).ConfigureAwait(false);

            await UpdateTeacher().ConfigureAwait(false);

            return new TeacherUpdateResultDto
            {
                Teacher = mapper.Map<TeacherDTO>(teacher),
                UploadingAvatarImageResult = changingAvatarResult?.UploadingResult?.OperationResult,
            };
        }

        private async Task DeleteProcessAsync(Guid id)
        {
            logger.LogInformation($"Deleting Teacher with Id = {id} started.");

            var entity = await teacherRepository.GetById(id).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(entity.AvatarImageId))
            {
                var removingResult = await imageService.RemoveImageAsync(entity.AvatarImageId).ConfigureAwait(false);

                if (!removingResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unreal to delete {nameof(Teacher)} [id = {id}] because unable to delete images.");
                }
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

        private async Task<ImageChangingResult> ChangeAvatarImage(Teacher teacher, string dtoImageId, IFormFile newImage)
        {
            ImageChangingResult changingAvatarResult = null;
            if (!string.Equals(dtoImageId, teacher.AvatarImageId, StringComparison.Ordinal)
                || (string.IsNullOrEmpty(dtoImageId) && newImage != null))
            {
                changingAvatarResult = await imageService.ChangeImageAsync(teacher.AvatarImageId, newImage).ConfigureAwait(false);

                teacher.AvatarImageId = changingAvatarResult.UploadingResult switch
                {
                    null when changingAvatarResult.RemovingResult is { Succeeded: false } => teacher.AvatarImageId,
                    { Succeeded: true } => changingAvatarResult.UploadingResult.Value,
                    _ => null
                };
            }

            return changingAvatarResult;
        }

        private async Task UpdateTeacher()
        {
            try
            {
                await teacherRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Unreal to update teacher.");
                throw;
            }
        }

        private async Task<TResult> RunInDefaultTransactionWithResultAsync<T, TResult>(Func<T, Task<TResult>> operation, T dto)
        {
            var strategy = executionStrategyHelper.CreateStrategyByDbName(DbContextName.OutOfSchoolDbContext);
            return await strategy.ExecuteAsync(async () =>
                await transactionProcessor.RunTransactionWithAutoCommitOrRollBackAsync(
                    new[] { DbContextName.OutOfSchoolDbContext, DbContextName.FilesDbContext },
                    async () => await operation(dto).ConfigureAwait(false)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task RunInDefaultTransactionAsync<T>(Func<T, Task> operation, T param)
        {
            var strategy = executionStrategyHelper.CreateStrategyByDbName(DbContextName.OutOfSchoolDbContext);
            await strategy.ExecuteAsync(async () =>
                await transactionProcessor.RunTransactionWithAutoCommitOrRollBackAsync(
                    new[] { DbContextName.OutOfSchoolDbContext, DbContextName.FilesDbContext },
                    async () => await operation(param).ConfigureAwait(false)).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task<TResult> ExecuteOperationWithResultAsync<T, TResult>(Func<T, Task<TResult>> operation, T param, bool enabledTransaction)
        {
            if (enabledTransaction)
            {
                return await RunInDefaultTransactionWithResultAsync(operation, param).ConfigureAwait(false);
            }

            return await operation(param).ConfigureAwait(false);
        }

        private async Task ExecuteOperationAsync<T>(Func<T, Task> operation, T param, bool enabledTransaction)
        {
            if (enabledTransaction)
            {
                await RunInDefaultTransactionAsync(operation, param).ConfigureAwait(false);
            }

            await operation(param).ConfigureAwait(false);
        }
    }
}