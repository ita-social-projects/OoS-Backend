using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Teacher entity.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly IEntityRepository<Teacher> repository;
        private readonly ILogger<TeacherService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Teacher entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public TeacherService(IEntityRepository<Teacher> repository, ILogger<TeacherService> logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Create(TeacherDTO dto)
        {
            logger.LogInformation("Teacher creating was started.");

            var teacher = dto.ToDomain();

            var newTeacher = await repository.Create(teacher).ConfigureAwait(false);

            logger.LogInformation($"Teacher with Id = {newTeacher?.Id} created successfully.");

            return newTeacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherDTO>> GetAll()
        {
            logger.LogInformation("Getting all Teachers started.");

            var teachers = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!teachers.Any()
                ? "Teacher table is empty."
                : $"All {teachers.Count()} records were successfully received from the Teacher table");

            return teachers.Select(teacher => teacher.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> GetById(long id)
        {
            logger.LogInformation($"Getting Teacher by Id started. Looking Id = {id}.");

            var teacher = await repository.GetById(id).ConfigureAwait(false);

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
        public async Task<TeacherDTO> Update(TeacherDTO dto)
        {
            logger.LogInformation($"Updating Teacher with Id = {dto?.Id} started.");

            try
            {
                var teacher = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"Teacher with Id = {teacher?.Id} updated succesfully.");

                return teacher.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Teacher with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.LogInformation($"Deleting Teacher with Id = {id} started.");

            var entity = new Teacher() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"Teacher with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting failed. Teacher with Id = {id} doesn't exist in the system.");
                throw;
            }
        }
    }
}