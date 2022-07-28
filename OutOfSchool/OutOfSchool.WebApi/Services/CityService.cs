using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using H3Lib;
using H3Lib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for for City entity.
/// </summary>
public class CityService : ICityService
{
    private readonly IEntityRepository<long, City> repository;
    private readonly ILogger<CityService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CityService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public CityService(IEntityRepository<long, City> repository, ILogger<CityService> logger, IStringLocalizer<SharedResource> localizer, IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CityDto>> GetAll()
    {
        logger.LogInformation("Getting all Cities started.");

        var cities = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!cities.Any()
            ? "City table is empty."
            : $"All {cities.Count()} records were successfully received from the City table");

        return cities.Select(city => mapper.Map<CityDto>(city)).ToList();
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

        return mapper.Map<CityDto>(city);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CityDto>> GetByCityName(string name)
    {
        logger.LogInformation("Getting all Cities by name started.");

        var cities = await repository.GetByFilter(c => c.Name.StartsWith(name)).ConfigureAwait(false);

        logger.LogInformation(!cities.Any()
            ? "City table is empty."
            : $"All {cities.Count()} records were successfully received from the City table");

        return cities.Select(city => mapper.Map<CityDto>(city)).ToList();
    }

    /// <inheritdoc/>
    public async Task<CityDto> GetNearestCityByFilter(CityFilter filter = null)
    {
        logger.LogInformation("Getting the nearest city by the current filter started.");
        if (filter is null)
        {
            filter = new CityFilter();
        }

        var geo = default(GeoCoord).SetDegrees(filter.Latitude, filter.Longitude);

        var h3Location = Api.GeoToH3(geo, GeoMathHelper.ResolutionForCity);
        Api.KRing(h3Location, GeoMathHelper.KRingForResolution, out var neighbours);

        var closestCities = await repository.GetByFilter(c => neighbours
                .Select(n => n.Value)
                .Any(hash => hash == c.GeoHash))
            .ConfigureAwait(false);

        var nearestCity = closestCities
            .Select(city => new
            {
                city,
                Distance = GeoMathHelper
                    .GetDistanceFromLatLonInKm(
                        (double)city.Latitude,
                        (double)city.Longitude,
                        (double)filter.Latitude,
                        (double)filter.Longitude),
            })
            .OrderBy(p => p.Distance)
            .Select(c => c.city)
            .FirstOrDefault();

        string currentFilterText = $"(Latitude = {filter.Latitude}, Longitude = {filter.Longitude})";

        logger.LogInformation(nearestCity is null
            ? $"There is no the nearest city for the filter {currentFilterText}."
            : $"The nearest city for the filter {currentFilterText} was successfully received.");

        return mapper.Map<CityDto>(nearestCity);
    }

    /// <inheritdoc/>
    public async Task<CityDto> Create(CityDto dto)
    {
        logger.LogInformation("City creating was started.");

        var city = mapper.Map<City>(dto).AddGeoHash();

        var newCity = await repository.Create(city).ConfigureAwait(false);

        logger.LogInformation($"City with Id = {newCity?.Id} created successfully.");

        return mapper.Map<CityDto>(newCity);
    }

    /// <inheritdoc/>
    public async Task<CityDto> Update(CityDto dto)
    {
        logger.LogInformation($"Updating City with Id = {dto?.Id} started.");

        try
        {
            var city = mapper.Map<City>(dto).AddGeoHash();

            var updatedCity = await repository.Update(city).ConfigureAwait(false);

            logger.LogInformation($"City with Id = {updatedCity?.Id} updated succesfully.");

            return mapper.Map<CityDto>(updatedCity);
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