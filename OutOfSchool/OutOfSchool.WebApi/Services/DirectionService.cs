﻿using System;
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
        private readonly IDirectionRepository repository;
        private readonly IWorkshopRepository repositoryWorkshop;
        private readonly ILogger<DirectionService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for Direction entity.</param>
        /// <param name="repositoryWorkshop">Workshop repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public DirectionService(
            IDirectionRepository repository,
            IWorkshopRepository repositoryWorkshop,
            ILogger<DirectionService> logger,
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
        public async Task<DirectionDto> Create(DirectionDto dto)
        {
            logger.LogInformation("Direction creating was started.");

            var direction = mapper.Map<Direction>(dto);

            DirectionValidation(dto);

            var newDirection = await repository.Create(direction).ConfigureAwait(false);

            logger.LogInformation($"Direction with Id = {newDirection?.Id} created successfully.");

            return mapper.Map<DirectionDto>(newDirection);
        }

        /// <inheritdoc/>
        public async Task<Result<DirectionDto>> Delete(long id)
        {
            logger.LogInformation($"Deleting Direction with Id = {id} started.");

            var entity = new Direction() { Id = id };

            var workShops = await repositoryWorkshop
                .GetByFilter(w => w.DirectionId == id)
                .ConfigureAwait(false);

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

                return Result<DirectionDto>.Success(mapper.Map<DirectionDto>(entity));
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

            var directions = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!directions.Any()
                ? "Direction table is empty."
                : $"All {directions.Count()} records were successfully received from the Direction table.");

            return directions.Select(entity => mapper.Map<DirectionDto>(entity)).ToList();
        }

        public async Task<SearchResult<DirectionDto>> GetByFilter(DirectionFilter filter)
        {
            logger.LogInformation("Getting Directions by filter started.");

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


            var directions = await repository.GetPagedByFilter(filter.From, filter.Size, predicate)
                .ConfigureAwait(false);

            logger.LogInformation($"All {directions.Count()} records were successfully received from the Direction table.");

            var result = new SearchResult<DirectionDto>()
            {
                TotalAmount = count,
                Entities = directions.Select(direction => mapper.Map<DirectionDto>(direction)).ToList(),
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

            return mapper.Map<DirectionDto>(direction);
        }

        /// <inheritdoc/>
        public async Task<DirectionDto> Update(DirectionDto dto)
        {
            logger.LogInformation($"Updating Direction with Id = {dto?.Id} started.");

            try
            {
                var direction = await repository.Update(mapper.Map<Direction>(dto)).ConfigureAwait(false);

                logger.LogInformation($"Direction with Id = {direction?.Id} updated succesfully.");

                return mapper.Map<DirectionDto>(direction);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Direction with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
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
