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
    /// Implements the interface with CRUD functionality for for City entity.
    /// </summary>
    public class CityService : ICityService
    {
        private readonly IEntityRepository<City> repository;
        private readonly ILogger<CityService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CityService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public CityService(IEntityRepository<City> repository, ILogger<CityService> logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CityDto>> GetAll()
        {
            logger.LogInformation("Getting all Cities started.");

            var cities = await repository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!cities.Any()
                ? "City table is empty."
                : $"All {cities.Count()} records were successfully received from the City table");

            return cities.Select(city => city.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<CityDto> GetById(long id)
        {
            logger.LogInformation($"Getting City by Id started. Looking Id = {id}.");

            var city = await repository.GetById(id).ConfigureAwait(false);

            if (city == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a City with Id = {id}.");

            return city.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CityDto>> GetByCityName(string name)
        {
            logger.LogInformation("Getting all Cities by name started.");

            var cities = await repository.GetByFilter(c => c.Name.StartsWith(name)).ConfigureAwait(false);

            logger.LogInformation(!cities.Any()
                ? "City table is empty."
                : $"All {cities.Count()} records were successfully received from the City table");

            return cities.Select(city => city.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<CityDto> Create(CityDto dto)
        {
            logger.LogInformation("City creating was started.");

            var city = dto.ToDomain();

            var newcity = await repository.Create(city).ConfigureAwait(false);

            logger.LogInformation($"City with Id = {newcity?.Id} created successfully.");

            return newcity.ToModel();
        }

        /// <inheritdoc/>
        public async Task<CityDto> Update(CityDto dto)
        {
            logger.LogInformation($"Updating City with Id = {dto?.Id} started.");

            try
            {
                var city = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"City with Id = {city?.Id} updated succesfully.");

                return city.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. City with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.LogInformation($"Deleting City with Id = {id} started.");

            var city = await repository.GetById(id).ConfigureAwait(false);

            if (city == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer[$"City with Id = {id} doesn't exist in the system"]);
            }

            await repository.Delete(city).ConfigureAwait(false);

            logger.LogInformation($"City with Id = {id} succesfully deleted.");
        }
    }
}
