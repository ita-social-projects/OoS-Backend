using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the operations to get popular workshops and categories.
/// </summary>
public class StatisticService : IStatisticService
{
    private readonly IApplicationRepository applicationRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IEntityRepositorySoftDeleted<long, Direction> directionRepository;
    private readonly ILogger<StatisticService> logger;
    private readonly IMapper mapper;

    // TODO: Maybe, we have to use an IMemoryCacheService.
    private readonly ICacheService cache;
    private readonly IAverageRatingService averageRatingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticService"/> class.
    /// </summary>
    /// <param name="applicationRepository">Application repository.</param>
    /// <param name="workshopRepository">Workshop repository.</param>
    /// <param name="directionRepository">Direction repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Automapper DI service.</param>
    /// <param name="cache">Redis cache service.</param>
    /// /// <param name="averageRatingService">Average rating service.</param>
    public StatisticService(
        IApplicationRepository applicationRepository,
        IWorkshopRepository workshopRepository,
        IEntityRepositorySoftDeleted<long, Direction> directionRepository,
        ILogger<StatisticService> logger,
        IMapper mapper,
        ICacheService cache,
        IAverageRatingService averageRatingService)
    {
        this.applicationRepository = applicationRepository;
        this.workshopRepository = workshopRepository;
        this.directionRepository = directionRepository;
        this.logger = logger;
        this.mapper = mapper;
        this.cache = cache;
        this.averageRatingService = averageRatingService;
    }

    // Return categories with 1 SQL query

    /// <inheritdoc/>
    public async Task<IEnumerable<DirectionDto>> GetPopularDirections(int limit, long catottgId)
    {
        logger.LogInformation("Getting popular categories started.");

        string cacheKey = $"GetPopularDirections_{limit}_{catottgId}";

        var popularDirections = await cache.GetOrAddAsync(cacheKey, () =>
            GetPopularDirectionsFromDatabase(limit, catottgId)).ConfigureAwait(false);

        return popularDirections;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DirectionDto>> GetPopularDirectionsFromDatabase(int limit, long catottgId)
    {
        var workshops = workshopRepository.Get(
            whereExpression: w => !w.IsBlocked && Provider.ValidProviderStatuses.Contains(w.Provider.Status));

        var applications = applicationRepository.Get();

        if (catottgId > 0)
        {
            workshops = workshops
                .Where(w =>
                    w.Address.CATOTTGId == catottgId
                    || (w.Address.CATOTTG.Category == CodeficatorCategory.CityDistrict.Name && w.Address.CATOTTG.ParentId == catottgId));
        }

        var directionsWithWorkshops = workshops
            .SelectMany(w => w.InstitutionHierarchy.Directions)
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.Id)
            .Select(g => new
            {
                DirectionId = g.Key,
                WorkshopsCount = g.Count() as int?,
            });

        var directionsWithApplications = applications
            .SelectMany(a => a.Workshop.InstitutionHierarchy.Directions)
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.Id)
            .Select(g => new
            {
                DirectionId = g.Key,
                ApplicationsCount = g.Count() as int?,
            });

        var directionsWithCounts = directionsWithWorkshops
            .GroupJoin(
                directionsWithApplications,
                directionWithWorkshop => directionWithWorkshop.DirectionId,
                directionWithApplication => directionWithApplication.DirectionId,
                (directionWithWorkshop, localDirectionsWithApplications) => new
                {
                    directionWithWorkshop,
                    localDirectionsWithApplications,
                })
            .SelectMany(
                x => x.localDirectionsWithApplications.DefaultIfEmpty(),
                (x, y) => new
                {
                    DirectionId = x.directionWithWorkshop.DirectionId,
                    ApplicationsCount = y.ApplicationsCount,
                    WorkshopsCount = x.directionWithWorkshop.WorkshopsCount,
                });

        var allDirections = directionRepository.Get();

        var statistics = allDirections
            .GroupJoin(
                directionsWithCounts,
                direction => direction.Id,
                directionWithCounts => directionWithCounts.DirectionId,
                (direction, localDirectionsWithCounts) => new { direction, localDirectionsWithCounts })
            .SelectMany(
                x => x.localDirectionsWithCounts,
                (x, y) => new
                {
                    Direction = mapper.Map<DirectionDto>(x.direction).WithCount(y.WorkshopsCount ?? 0),
                    ApplicationsCount = y.ApplicationsCount ?? 0,
                });

        var sortedStatistics = await statistics
            .OrderByDescending(s => s.ApplicationsCount)
            .Take(limit)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation($"All {sortedStatistics.Count} records were successfully received");

        return sortedStatistics.Select(s => s.Direction);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkshopCard>> GetPopularWorkshops(int limit, long catottgId)
    {
        logger.LogInformation("Getting popular workshops started.");

        string cacheKey = $"GetPopularWorkshops_{limit}_{catottgId}";

        var workshopsResult = await cache.GetOrAddAsync(cacheKey, () =>
            GetPopularWorkshopsFromDatabase(limit, catottgId)).ConfigureAwait(false);

        return workshopsResult;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkshopCard>> GetPopularWorkshopsFromDatabase(int limit, long catottgId)
    {
        var workshops = workshopRepository
            .Get(
                includeProperties: $"{nameof(Address)},{nameof(InstitutionHierarchy)}",
                whereExpression: w => !w.IsBlocked && Provider.ValidProviderStatuses.Contains(w.Provider.Status) && !w.InstitutionHierarchy.IsDeleted);

        if (catottgId > 0)
        {
            workshops = workshops
                .Where(w => w.Address.CATOTTGId == catottgId || (w.Address.CATOTTG.Category == CodeficatorCategory.CityDistrict.Name && w.Address.CATOTTG.ParentId == catottgId));
        }

        var workshopsWithApplications = workshops.Select(w => new
        {
            Workshop = w,
            Applications = w.Applications.Count,
        });

        var popularWorkshops = workshopsWithApplications
            .OrderByDescending(w => w.Applications)
            .Select(w => w.Workshop)
            .Include(w => w.Applications).ThenInclude(a => a.Child)
            .Include(w => w.Applications).ThenInclude(a => a.Parent)
            .Include(w => w.Provider)
            .Include(w => w.Address).ThenInclude(ad => ad.CATOTTG)
            .Include(w => w.InstitutionHierarchy).ThenInclude(i => i.Directions)
            .Include(w => w.InstitutionHierarchy).ThenInclude(i => i.Institution)
            .Take(limit)
            .AsNoTracking();

        var popularWorkshopsList = await popularWorkshops.ToListAsync().ConfigureAwait(false);

        logger.LogInformation($"All {popularWorkshopsList.Count} records were successfully received");

        var workshopsCard = mapper.Map<List<WorkshopCard>>(popularWorkshopsList);

        var result = await GetWorkshopsWithAverageRating(workshopsCard).ConfigureAwait(false);

        return result;
    }

    private async Task<List<WorkshopCard>> GetWorkshopsWithAverageRating(List<WorkshopCard> workshopsCards)
    {
        var averageRatings = await averageRatingService.GetByEntityIdsAsync(workshopsCards.Select(p => p.WorkshopId)).ConfigureAwait(false);

        foreach (var workshop in workshopsCards)
        {
            var averageRatingDto = averageRatings?.SingleOrDefault(r => r.EntityId == workshop.WorkshopId);
            workshop.Rating = averageRatingDto?.Rate ?? default;
            workshop.NumberOfRatings = averageRatingDto?.RateQuantity ?? default;
        }

        return workshopsCards;
    }
}