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

            return newTeacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TeacherDTO>> GetAll()
        {
            logger.Information("Process of getting all Teachers started.");

            var teachers = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!teachers.Any()
                ? "Teacher table is empty."
                : "Successfully got all records from the Teacher table.");

            return teachers.Select(teacher => teacher.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> GetById(long id)
        {
            logger.Information("Process of getting Teacher by id started.");

            var teacher = await repository.GetById(id).ConfigureAwait(false);

            if (teacher == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Teacher with id = {id}.");

            return teacher.ToModel();
        }

        /// <inheritdoc/>
        public async Task<TeacherDTO> Update(TeacherDTO dto)
        {
            logger.Information("Teacher updating was launched.");

            try
            {
                var teacher = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Teacher successfully updated.");

                return teacher.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Teacher in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Teacher deleting was launched.");

            var entity = new Teacher() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Teacher successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Teacher in the Db with such an id.");
                throw;
            }
        }
    }
}