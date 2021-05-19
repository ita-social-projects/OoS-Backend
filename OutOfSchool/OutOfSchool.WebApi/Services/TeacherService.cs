using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Teacher entity.
    /// </summary>
    public class TeacherService : ITeacherService
    {
        private readonly IEntityRepository<Teacher> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Teacher entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public TeacherService(IEntityRepository<Teacher> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Create(TeacherDTO dto)
        {
            logger.Information("Teacher creating was started.");

            var teacher = dto.ToDomain();

            var newTeacher = await repository.Create(teacher).ConfigureAwait(false);

            logger.Information($"Teacher with Id = {newTeacher?.Id} created successfully.");

            return newTeacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherDTO>> GetAll()
        {
            logger.Information("Getting all Teachers started.");

            var teachers = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!teachers.Any()
                ? "Teacher table is empty."
                : $"All {teachers.Count()} records were successfully received from the Teacher table");

            return teachers.Select(teacher => teacher.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> GetById(long id)
        {
            logger.Information($"Getting Teacher by Id started. Looking Id = {id}.");

            var teacher = await repository.GetById(id).ConfigureAwait(false);

            if (teacher == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Teacher with Id = {id}.");

            return teacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Update(TeacherDTO dto)
        {
            logger.Information($"Updating Teacher with Id = {dto?.Id} started.");

            try
            {
                var teacher = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Teacher with Id = {teacher?.Id} updated succesfully.");

                return teacher.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Teacher with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Teacher with Id = {id} started.");

            var entity = new Teacher() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Teacher with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Teacher with Id = {id} doesn't exist in the system.");
                throw;
            }
        }
    }
}