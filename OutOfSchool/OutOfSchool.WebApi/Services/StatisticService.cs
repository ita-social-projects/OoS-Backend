using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Redis;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the operations to get popular workshops and categories.
/// </summary>
public class StatisticService : IStatisticService
{
    private readonly IApplicationRepository applicationRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IRatingService ratingService;
    private readonly IDirectionRepository directionRepository;
    private readonly ILogger<StatisticService> logger;
    private readonly IMapper mapper;
    private readonly ICacheService cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticService"/> class.
    /// </summary>
    /// <param name="applicationRepository">Application repository.</param>
    /// <param name="workshopRepository">Workshop repository.</param>
    /// <param name="ratingService">Rating service.</param>
    /// <param name="directionRepository">Direction repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Automapper DI service.</param>
    /// <param name="cache">Redis cache service.</param>
    public StatisticService(
        IApplicationRepository applicationRepository,
        IWorkshopRepository workshopRepository,
        IRatingService ratingService,
        IDirectionRepository directionRepository,
        ILogger<StatisticService> logger,
        IMapper mapper,
        ICacheService cache)
    {
        this.applicationRepository = applicationRepository;
        this.workshopRepository = workshopRepository;
        this.ratingService = ratingService;
        this.directionRepository = directionRepository;
        this.logger = logger;
        this.mapper = mapper;
        this.cache = cache;
    }

    // Return categories with 1 SQL query

    /// <inheritdoc/>
    public async Task<IEnumerable<DirectionStatistic>> GetPopularDirections(int limit, string city)
    {
        logger.LogInformation("Getting popular categories started.");

        string cacheKey = $"GetPopularDirections_{limit}_{city}";

        var popularDirections = await cache.GetOrAddAsync(cacheKey, () =>
            GetPopularDirectionsFromDatabase(limit, city)).ConfigureAwait(false);

        return popularDirections;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DirectionStatistic>> GetPopularDirectionsFromDatabase(int limit, string city)
    {
        // var workshops = workshopRepository.Get();
        // var applications = applicationRepository.Get();
        //
        // if (!string.IsNullOrWhiteSpace(city))
        // {
        //     workshops = workshops
        //         .Where(w => string.Equals(w.Address.City, city.Trim()));
        // }
        //
        // var directionsWithWorkshops = workshops
        //     .GroupBy(w => w.DirectionId)
        //     .Select(g => new
        //     {
        //         DirectionId = g.Key,
        //         WorkshopsCount = g.Count() as int?,
        //     });
        //
        // var directionsWithApplications = applications
        //     .GroupBy(a => a.Workshop.DirectionId)
        //     .Select(g => new
        //     {
        //         DirectionId = g.Key,
        //         ApplicationsCount = g.Count() as int?,
        //     });
        //
        // var directionsWithCounts = directionsWithWorkshops
        //     .GroupJoin(
        //         directionsWithApplications,
        //         directionWithWorkshop => directionWithWorkshop.DirectionId,
        //         directionWithApplication => directionWithApplication.DirectionId,
        //         (directionWithWorkshop, localDirectionsWithApplications) => new
        //         {
        //             directionWithWorkshop,
        //             localDirectionsWithApplications,
        //         })
        //     .SelectMany(
        //         x => x.localDirectionsWithApplications.DefaultIfEmpty(),
        //         (x, y) => new
        //         {
        //             DirectionId = x.directionWithWorkshop.DirectionId,
        //             ApplicationsCount = y.ApplicationsCount,
        //             WorkshopsCount = x.directionWithWorkshop.WorkshopsCount,
        //         });
        //
        // var allDirections = directionRepository.Get();
        //
        // var statistics = allDirections
        //     .GroupJoin(
        //         directionsWithCounts,
        //         direction => direction.Id,
        //         directionWithCounts => directionWithCounts.DirectionId,
        //         (direction, localDirectionsWithCounts) => new { direction, localDirectionsWithCounts })
        //     .SelectMany(
        //         x => x.localDirectionsWithCounts,
        //         (x, y) => new DirectionStatistic
        //         {
        //             Direction = mapper.Map<DirectionDto>(x.direction),
        //             ApplicationsCount = y.ApplicationsCount ?? 0,
        //             WorkshopsCount = y.WorkshopsCount ?? 0,
        //         });
        //
        // var sortedStatistics = await statistics
        //     .OrderByDescending(s => s.ApplicationsCount)
        //     .Take(limit)
        //     .ToListAsync()
        //     .ConfigureAwait(false);
        //
        // logger.LogInformation($"All {sortedStatistics.Count} records were successfully received");
        //
        // return sortedStatistics;
        return Enumerable.Empty<DirectionStatistic>();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkshopCard>> GetPopularWorkshops(int limit, string city)
    {
        logger.LogInformation("Getting popular workshops started.");

        string cacheKey = $"GetPopularWorkshops_{limit}_{city}";

        var workshopsResult = await cache.GetOrAddAsync(cacheKey, () =>
            GetPopularWorkshopsFromDatabase(limit, city)).ConfigureAwait(false);

        return workshopsResult;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkshopCard>> GetPopularWorkshopsFromDatabase(int limit, string city)
    {
        var workshops = workshopRepository
            .Get(includeProperties: $"{nameof(Address)},{nameof(Direction)}");

        if (!string.IsNullOrWhiteSpace(city))
        {
            workshops = workshops
                .Where(w => string.Equals(w.Address.City, city.Trim()));
        }

        var workshopsWithApplications = workshops.Select(w => new
        {
            Workshop = w,
            Applications = w.Applications.Count,
        });

        var popularWorkshops = workshopsWithApplications
            .OrderByDescending(w => w.Applications)
            .Select(w => w.Workshop)
            .Take(limit);

        var popularWorkshopsList = await popularWorkshops.ToListAsync().ConfigureAwait(false);

        logger.LogInformation($"All {popularWorkshopsList.Count} records were successfully received");

        var workshopsCard = mapper.Map<List<WorkshopCard>>(popularWorkshopsList);

        var result = GetWorkshopsWithAverageRating(workshopsCard);

        return result;
    }

    private List<WorkshopCard> GetWorkshopsWithAverageRating(List<WorkshopCard> workshopsCards)
    {
        var averageRatings =
            ratingService.GetAverageRatingForRange(workshopsCards.Select(p => p.WorkshopId), RatingType.Workshop);

        if (averageRatings != null)
        {
            foreach (var workshop in workshopsCards)
            {
                var ratingTuple = averageRatings.FirstOrDefault(r => r.Key == workshop.WorkshopId);
                workshop.Rating = ratingTuple.Value?.Item1 ?? default;
            }
        }

        return workshopsCards;
    }
}