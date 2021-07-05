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
    /// Implements the interface with CRUD functionality for Department entity.
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public DepartmentService(IDepartmentRepository entityRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<DepartmentDto> Create(DepartmentDto dto)
        {
            logger.Information("Department creating was started.");

            var department = dto.ToDomain();

            ModelValidation(dto);

            var newDepartment = await repository.Create(department).ConfigureAwait(false);

            logger.Information($"Department with Id = {newDepartment?.Id} created successfully.");

            return newDepartment.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Department with Id = {id} started.");

            var entity = new Department() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Department with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Department with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DepartmentDto>> GetAll()
        {
            logger.Information("Getting all Departments started.");

            var departments = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!departments.Any()
                ? "Department table is empty."
                : $"All {departments.Count()} records were successfully received from the Department table");

            return departments.Select(entity => entity.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<DepartmentDto> GetById(long id)
        {
            logger.Information($"Getting Department by Id started. Looking Id = {id}.");

            var department = await repository.GetById((int)id).ConfigureAwait(false);

            if (department == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Department with Id = {id}.");

            return department.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DepartmentDto>> GetByDirectionId(long id)
        {
            logger.Information($"Getting Department by DirectionId started. Looking DirectionId = {id}.");

            IdValidation(id);

            var departments = await this.repository.Get<int>(where: x => x.DirectionId == id).ToListAsync().ConfigureAwait(false);

            logger.Information(!departments.Any()
                ? $"There is no Deparment with DirectionId = {id}."
                : $"All {departments.Count} records were successfully received from the Department table");

            return departments.Select(entity => entity.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<DepartmentDto> Update(DepartmentDto dto)
        {
            logger.Information($"Updating the Department with Id = {dto?.Id} started.");

            ModelValidation(dto);

            try
            {
                var department = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Department with Id = {department?.Id} updated succesfully.");

                return department.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Department with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        private void ModelValidation(DepartmentDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException(localizer["Object cannot be null."]);
            }

            if (!repository.DirectionExists(dto.DirectionId))
            {
                throw new ArgumentException(localizer["There is no Direction with such id."]);
            }

            if (repository.SameExists(dto.ToDomain()))
            {
                throw new ArgumentException(localizer["There is already a Department with such a data."]);
            }
        }

        private void IdValidation(long id)
        {
            if (!repository.DirectionExists(id))
            {
                throw new ArgumentException(localizer["There is no Department with such id."]);
            }
        }
    }
}
