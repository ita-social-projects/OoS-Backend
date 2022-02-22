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
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Direction entity.
    /// </summary>
    public class DirectionService : IDirectionService
    {
        private readonly IEntityRepository<Direction> repository;
        private readonly IWorkshopRepository repositoryWorkshop;
        private readonly ILogger<DirectionService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;
        private readonly string includePropertiesForGetByFilterWithName = $"";

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for Direction entity.</param>
        /// <param name="repositoryWorkshop">Workshop repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public DirectionService(
            IEntityRepository<Direction> entityRepository,
            IWorkshopRepository repositoryWorkshop,
            ILogger<DirectionService> logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.repositoryWorkshop = repositoryWorkshop;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<DirectionDto> Create(DirectionDto dto)
        {
            logger.LogInformation("Direction creating was started.");

            var direction = dto.ToDomain();

            DirectionValidation(dto);

            var newDirection = await repository.Create(direction).ConfigureAwait(false);

            logger.LogInformation($"Direction with Id = {newDirection?.Id} created successfully.");

            return newDirection.ToModel();
        }

        /// <inheritdoc/>
        public async Task<Result<DirectionDto>> Delete(long id)
        {
            logger.LogInformation($"Deleting Direction with Id = {id} started.");

            var entity = new Direction() { Id = id };

            var workShops = await repositoryWorkshop.GetByFilter(w => w.DirectionId == id).ConfigureAwait(false);
            if (workShops.Any())
            {
                return Result<DirectionDto>.Failed(new OperationError
                    {
                        Code = "400",
                        Description = localizer["Some workshops assosiated with this direction. Deletion prohibited."],
                    });
            }

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"Direction with Id = {id} succesfully deleted.");

                return Result<DirectionDto>.Success(entity.ToModel());
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting failed. Direction with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DirectionDto>> GetAll()
        {
            logger.LogInformation("Getting all Directions started.");

            var directions = await this.repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!directions.Any()
                ? "Direction table is empty."
                : $"All {directions.Count()} records were successfully received from the Direction table.");

            return directions.Select(entity => entity.ToModel()).ToList();
        }

        public async Task<SearchResult<DirectionDto>> GetByFilter(DirectionFilter filter)
        {
            logger.LogInformation("Getting Directions by filter started.");
            int count = 0;
            var directions = new List<Direction>();
            if (!string.IsNullOrEmpty(filter.Name))
            {
                var predicate = filter.Name switch
                {
                    var name when string.IsNullOrWhiteSpace(name) => PredicateBuilder.True<Direction>(),
                    _ => PredicateBuilder
                        .False<Direction>()
                        .Or(direction =>
                            direction.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase))
                        .Or(direction => direction.Departments.Any(department =>
                            department.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase)))
                        .Or(direction => direction.Departments.Any(department =>
                            department.Classes.Any(c =>
                                c.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase)))),
                };
                directions = await repository.Get<int>(filter.From, filter.Size, where: predicate).ToListAsync().ConfigureAwait(false);
                count = await repository.Count(predicate).ConfigureAwait(false);

            }
            else
            {
                count = await repository.Count().ConfigureAwait(false);
                directions = await this.repository
                    .Get<int>(filter.From, filter.Size)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            logger.LogInformation(!directions.Any()
                ? "Direction table is empty."
                : $"All {directions.Count()} records were successfully received from the Direction table.");

            var result = new SearchResult<DirectionDto>()
            {
                TotalAmount = count,
                Entities = directions.Select(entity => entity.ToModel()).ToList(),
            };

            return result;
        }

        /// <inheritdoc/>
        public async Task<DirectionDto> GetById(long id)
        {
            logger.LogInformation($"Getting Direction by Id started. Looking Id = {id}.");

            var direction = await repository.GetById((int)id).ConfigureAwait(false);

            if (direction == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a Direction with Id = {id}.");

            return direction.ToModel();
        }

        /// <inheritdoc/>
        public async Task<DirectionDto> Update(DirectionDto dto)
        {
            logger.LogInformation($"Updating Direction with Id = {dto?.Id} started.");

            try
            {
                var direction = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"Direction with Id = {direction?.Id} updated succesfully.");

                return direction.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Direction with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        public async Task<SearchResult<DirectionDto>> FilterByName(DirectionFilter filter)
        {
            var predicate = filter.Name switch
            {
                var name when string.IsNullOrWhiteSpace(name) => PredicateBuilder.True<Direction>(),
                _ => PredicateBuilder
                    .False<Direction>()
                    .Or(direction => direction.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase))
                    .Or(direction => direction.Departments.Any(department =>
                        department.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase)))
                    .Or(direction => direction.Departments.Any(department =>
                        department.Classes.Any(c =>
                            c.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase)))),
            };
            var count = await repository.Count(predicate).ConfigureAwait(false);


            var directions = await repository.Get<int>(filter.Size, filter.From, where: predicate)
                .ToListAsync()
                .ConfigureAwait(false);

            var result = new SearchResult<DirectionDto>()
            {
                TotalAmount = count,
                Entities = directions.Select(direction => mapper.Map<DirectionDto>(direction)).ToList(),
            };

            return result;
        }

        private void DirectionValidation(DirectionDto dto)
        {
            if (repository.Get<int>(where: x => x.Title == dto.Title).Any())
            {
                throw new ArgumentException(localizer["There is already a Direction with such a data."]);
            }
        }
    }
}
