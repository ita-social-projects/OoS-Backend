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
    /// Implements the interface with CRUD functionality for Class entity.
    /// </summary>
    public class ClassService : IClassService
    {
        private readonly IClassRepository repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ClassService(IClassRepository entityRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ClassDto> Create(ClassDto dto)
        {
            logger.Information("Class creating was started.");

            var classEntity = dto.ToDomain();

            ModelValidation(dto);

            var newClass = await repository.Create(classEntity).ConfigureAwait(false);

            logger.Information($"Class with Id = {newClass?.Id} created successfully.");

            return newClass.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Class with Id = {id} started.");

            var entity = new Class() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Class with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Class with such Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClassDto>> GetAll()
        {
            logger.Information("Getting all Classes started.");

            var classes = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!classes.Any()
                ? "Class table is empty."
                : $"All {classes.Count()} records were successfully received from the Class table");

            return classes.Select(entity => entity.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ClassDto> GetById(long id)
        {
            logger.Information($"Getting Class by Id started. Looking Id = {id}.");

            var classEntity = await repository.GetById((int)id).ConfigureAwait(false);

            if (classEntity == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Class with Id = {id}.");

            return classEntity.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClassDto>> GetByDepartmentId(long id)
        {
            logger.Information($"Getting Class by Department's id started. Looking DepartmentId = {id}.");

            IdValidation(id);

            var classes = await this.repository.Get<int>(where: x => x.DepartmentId == id).ToListAsync().ConfigureAwait(false);

            logger.Information(!classes.Any()
                ? $"There aren't Classes for Department with Id = {id}."
                : $"All {classes.Count} records were successfully received from the Class table");

            return classes.Select(entity => entity.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ClassDto> Update(ClassDto dto)
        {
            logger.Information($"Updating Class with Id = {dto?.Id} started.");

            try
            {
                var classEntity = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Class with Id = {classEntity?.Id} updated succesfully.");

                return classEntity.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Class with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        private void ModelValidation(ClassDto dto)
        {
            if (repository.Get<int>(where: x => x.Title == dto.Title).Any())
            {
                throw new ArgumentException(localizer["There is already a Class with such a data."]);
            }
        }

        private void IdValidation(long id)
        {
            if (!repository.DepartmentExists(id))
            {
                throw new ArgumentException(localizer["There is no Department with such id."]);
            }
        }
    }
}
