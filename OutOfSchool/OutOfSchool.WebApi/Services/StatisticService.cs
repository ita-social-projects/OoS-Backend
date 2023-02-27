using AutoMapper;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
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
    private readonly IEntityRepository<long, Direction> directionRepository;
    private readonly ILogger<StatisticService> logger;
    private readonly IMapper mapper;
    private readonly ICacheService cache;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;

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
    /// <param name="currentUserService">Service for manage current user.</param>
    /// <param name="ministryAdminService">Service for manage ministry admin.</param>
    /// <param name="regionAdminService">Service for managing region admin rigths.</param>
    public StatisticService(
        IApplicationRepository applicationRepository,
        IWorkshopRepository workshopRepository,
        IRatingService ratingService,
        IEntityRepository<long, Direction> directionRepository,
        ILogger<StatisticService> logger,
        IMapper mapper,
        ICacheService cache,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService)
    {
        this.applicationRepository = applicationRepository;
        this.workshopRepository = workshopRepository;
        this.ratingService = ratingService;
        this.directionRepository = directionRepository;
        this.logger = logger;
        this.mapper = mapper;
        this.cache = cache;
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
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
        var workshops = workshopRepository.Get();
        var applications = applicationRepository.Get();

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            workshops = workshops
                .Where(w => w.InstitutionHierarchy.InstitutionId == ministryAdmin.InstitutionId);
            applications = applications
                .Where(a => a.Workshop.InstitutionHierarchy.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            workshops = workshops
                .Where(w => w.InstitutionHierarchy.InstitutionId == regionAdmin.InstitutionId);
            applications = applications
                .Where(a => a.Workshop.InstitutionHierarchy.InstitutionId == regionAdmin.InstitutionId);
        }

        if (catottgId > 0)
        {
            workshops = workshops
                .Where(w => w.Address.CATOTTGId == catottgId || (w.Address.CATOTTG.Category == CodeficatorCategory.CityDistrict.Name && w.Address.CATOTTG.ParentId == catottgId));
        }

        var directionsWithWorkshops = workshops
            .SelectMany(w => w.InstitutionHierarchy.Directions)
            .GroupBy(d => d.Id)
            .Select(g => new
            {
                DirectionId = g.Key,
                WorkshopsCount = g.Count() as int?,
            });

        var directionsWithApplications = applications
            .SelectMany(a => a.Workshop.InstitutionHierarchy.Directions)
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
            .Get(includeProperties: $"{nameof(Address)},{nameof(InstitutionHierarchy)}", where: w => !w.IsBlocked);

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            workshops = workshops
                .Where(w => w.InstitutionHierarchy.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            workshops = workshops
                .Where(w => w.InstitutionHierarchy.InstitutionId == regionAdmin.InstitutionId);
        }

        if (!currentUserService.IsAdmin())
        {
            workshops = workshops.Where(w => !w.IsBlocked);
        }

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
            .Take(limit);

        var popularWorkshopsList = await popularWorkshops.ToListAsync().ConfigureAwait(false);

        logger.LogInformation($"All {popularWorkshopsList.Count} records were successfully received");

        var workshopsCard = mapper.Map<List<WorkshopCard>>(popularWorkshopsList);

        var result = await GetWorkshopsWithAverageRating(workshopsCard).ConfigureAwait(false);

        return result;
    }

    private async Task<List<WorkshopCard>> GetWorkshopsWithAverageRating(List<WorkshopCard> workshopsCards)
    {
        var averageRatings =
            await ratingService.GetAverageRatingForRangeAsync(workshopsCards.Select(p => p.WorkshopId), RatingType.Workshop).ConfigureAwait(false);

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