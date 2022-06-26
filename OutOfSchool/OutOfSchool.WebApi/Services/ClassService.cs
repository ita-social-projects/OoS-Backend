using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Class entity.
    /// </summary>
    public class ClassService : IClassService
    {
        private readonly IClassRepository repository;
        private readonly IWorkshopRepository repositoryWorkshop;
        private readonly ILogger<ClassService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassService"/> class.
        /// </summary>
        /// <param name="repository">Repository for some entity.</param>
        /// <param name="repositoryWorkshop">Workshop repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public ClassService(
            IClassRepository repository,
            IWorkshopRepository repositoryWorkshop,
            ILogger<ClassService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.repositoryWorkshop = repositoryWorkshop;
            this.logger = logger;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<ClassDto> Create(ClassDto dto)
        {
            logger.LogInformation("Class creating was started.");

            var classEntity = mapper.Map<Class>(dto);

            ModelValidation(dto);

            var newClass = await repository.Create(classEntity).ConfigureAwait(false);

            logger.LogInformation($"Class with Id = {newClass?.Id} created successfully.");

            return mapper.Map<ClassDto>(newClass);
        }

        public async Task<List<ClassDto>> Create(ClassDto[] dtos)
        {
            logger.LogInformation("Classes creating was started.");
            List<ClassDto> addedArray = new List<ClassDto>();
            foreach (var dto in dtos)
            {
                var classEntity = mapper.Map<Class>(dto);
                ModelValidation(dto);
                var newClass = await repository.Create(classEntity).ConfigureAwait(false);
                logger.LogInformation($"Class with Id = {newClass?.Id} created successfully.");
                addedArray.Add(mapper.Map<ClassDto>(newClass));
            }

            return addedArray;
        }

        /// <inheritdoc/>
        public async Task<Result<ClassDto>> Delete(long id)
        {
            logger.LogInformation($"Deleting Class with Id = {id} started.");

            var entity = new Class() { Id = id };

            var workShops = await repositoryWorkshop.GetByFilter(w => w.ClassId == id).ConfigureAwait(false);
            if (workShops.Any())
            {
                return Result<ClassDto>.Failed(new OperationError
                {
                    Code = "400",
                    Description = localizer["Some workshops assosiated with this class. Deletion prohibited."],
                });
            }

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"Class with Id = {id} succesfully deleted.");

                return Result<ClassDto>.Success(mapper.Map<ClassDto>(entity));
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting failed. Class with such Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClassDto>> GetAll()
        {
            logger.LogInformation("Getting all Classes started.");

            var classes = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!classes.Any()
                ? "Class table is empty."
                : $"All {classes.Count()} records were successfully received from the Class table");

            return classes.Select(entity => mapper.Map<ClassDto>(entity)).ToList();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ClassDto>> GetByFilter(OffsetFilter filter)
        {
            logger.LogInformation("Getting all Classes started.");

            var count = await repository.Count().ConfigureAwait(false);

            var classes = await repository
                .Get<int>(filter.From, filter.Size)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogInformation(!classes.Any()
                ? "Class table is empty."
                : $"All {classes.Count} records were successfully received from the Class table");

            var result = new SearchResult<ClassDto>()
            {
                TotalAmount = count,
                Entities = classes.Select(entity => mapper.Map<ClassDto>(entity)).ToList(),
            };

            return result;
        }

        /// <inheritdoc/>
        public async Task<ClassDto> GetById(long id)
        {
            logger.LogInformation($"Getting Class by Id started. Looking Id = {id}.");

            var classEntity = await repository.GetById((int)id).ConfigureAwait(false);

            if (classEntity == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a Class with Id = {id}.");

            return mapper.Map<ClassDto>(classEntity);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClassDto>> GetByDepartmentId(long id)
        {
            logger.LogInformation($"Getting Class by Department's id started. Looking DepartmentId = {id}.");

            IdValidation(id);

            var classes = await repository
                .Get<int>(where: x => x.DepartmentId == id)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogInformation(!classes.Any()
                ? $"There aren't Classes for Department with Id = {id}."
                : $"All {classes.Count} records were successfully received from the Class table");

            return classes.Select(entity => mapper.Map<ClassDto>(entity)).ToList();
        }

        /// <inheritdoc/>
        public async Task<ClassDto> Update(ClassDto dto)
        {
            logger.LogInformation($"Updating Class with Id = {dto?.Id} started.");

            try
            {
                var classEntity = await repository.Update(mapper.Map<Class>(dto)).ConfigureAwait(false);

                logger.LogInformation($"Class with Id = {classEntity?.Id} updated succesfully.");

                return mapper.Map<ClassDto>(classEntity);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Class with Id = {dto?.Id} doesn't exist in the system.");
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
