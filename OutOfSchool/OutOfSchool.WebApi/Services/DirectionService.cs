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
    /// Implements the interface with CRUD functionality for Direction entity.
    /// </summary>
    public class DirectionService : IDirectionService
    {
        private readonly IEntityRepository<Direction> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectionService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for Direction entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public DirectionService(IEntityRepository<Direction> entityRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = entityRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<DirectionDto> Create(DirectionDto dto)
        {
            logger.Information("Direction creating was started.");

            var direction = dto.ToDomain();

            DirectionValidation(dto);

            var newDirection = await repository.Create(direction).ConfigureAwait(false);

            logger.Information($"Direction with Id = {newDirection?.Id} created successfully.");

            return newDirection.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Direction with Id = {id} started.");

            var entity = new Direction() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Direction with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Deleting failed. Direction with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DirectionDto>> GetAll()
        {
            logger.Information("Getting all Directions started.");

            var directions = await this.repository.GetAll().ConfigureAwait(false);

            logger.Information(!directions.Any()
                ? "Direction table is empty."
                : $"All {directions.Count()} records were successfully received from the Direction table.");

            return directions.Select(entity => entity.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<DirectionDto> GetById(long id)
        {
            logger.Information($"Getting Direction by Id started. Looking Id = {id}.");

            var direction = await repository.GetById((int)id).ConfigureAwait(false);

            if (direction == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Direction with Id = {id}.");

            return direction.ToModel();
        }

        /// <inheritdoc/>
        public async Task<DirectionDto> Update(DirectionDto dto)
        {
            logger.Information($"Updating Direction with Id = {dto?.Id} started.");

            try
            {
                var direction = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Direction with Id = {direction?.Id} updated succesfully.");

                return direction.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Direction with Id = {dto?.Id} doesn't exist in the system.");
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
