using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the operations to get popular workshops and categories.
/// </summary>
/// <param name="applicationRepository">Application repository.</param>
/// <param name="workshopRepository">Workshop repository.</param>
/// <param name="directionRepository">Direction repository.</param>
/// <param name="logger">Logger.</param>
/// <param name="cache">Redis cache service.</param>
/// /// <param name="averageRatingService">Average rating service.</param>
public class StatisticService(
    IApplicationRepository applicationRepository,
    IWorkshopRepository workshopRepository,
    ICompetitiveEventRepository competitiveEventRepository,
    IEntityRepositorySoftDeleted<long, Direction> directionRepository,
    ILogger<StatisticService> logger,
    ICacheService cache,
    IAverageRatingService averageRatingService
) : IStatisticService
{

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
                    w.Contacts.Any(c => c.IsDefault &&
                        (c.Address.CATOTTGId == catottgId ||
                        (c.Address.CATOTTG.Category == CodeficatorCategory.CityDistrict.Name &&
                         c.Address.CATOTTG.ParentId == catottgId))));
        }

        var directionsWithWorkshops = workshops
            .SelectMany(w => w.InstitutionHierarchy.SubDirections)
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.DirectionId)
            .Select(g => new
            {
                DirectionId = g.Key,
                WorkshopsCount = g.Count() as int?,
            });

        var directionsWithApplications = applications
            .SelectMany(a => a.Workshop.InstitutionHierarchy.SubDirections)
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.DirectionId)
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
                    Direction = x.direction.ToDto(y.WorkshopsCount ?? 0),
                    ApplicationsCount = y.ApplicationsCount ?? 0,
                });

        var sortedStatistics = await statistics
            .OrderByDescending(s => s.ApplicationsCount)
            .Take(limit)
            .Select(s => s.Direction)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation($"All {sortedStatistics.Count} records were successfully received");

        return sortedStatistics;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkshopCard>> GetPopularWorkshops(int limit, long catottgId)
    {
        logger.LogInformation("Getting popular workshops started.");

        var cacheKey = $"GetPopularWorkshops_{limit}_{catottgId}";

        var workshopsResult = await cache.GetOrAddAsync(cacheKey, () =>
            GetPopularWorkshopsFromDatabase(limit, catottgId)).ConfigureAwait(false);

        return workshopsResult;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkshopCard>> GetPopularWorkshopsFromDatabase(int limit, long catottgId)
    {
        var workshops = workshopRepository
            .Get(whereExpression: w => !w.IsBlocked && Provider.ValidProviderStatuses.Contains(w.Provider.Status) && !w.InstitutionHierarchy.IsDeleted)
            .Include(w => w.InstitutionHierarchy)
            .AsQueryable();

        if (catottgId > 0)
        {
            workshops = workshops
                .Where(w => w.Contacts.Any(c => c.IsDefault &&
                    (c.Address.CATOTTGId == catottgId ||
                    (c.Address.CATOTTG.Category == CodeficatorCategory.CityDistrict.Name &&
                     c.Address.CATOTTG.ParentId == catottgId))));
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
            .Include(w => w.Contacts).ThenInclude(c => c.Address.CATOTTG)
            .Include(w => w.InstitutionHierarchy).ThenInclude(i => i.SubDirections).ThenInclude(s => s.Direction)
            .Include(w => w.InstitutionHierarchy).ThenInclude(i => i.Institution)
            .Take(limit)
            .AsNoTracking();

        var popularWorkshopsList = await popularWorkshops.ToListAsync().ConfigureAwait(false);

        logger.LogInformation($"All {popularWorkshopsList.Count} records were successfully received");

        var workshopsCard = popularWorkshopsList.ToCard();

        await TakenSeatsMappingHelper.FillTakenSeatsForCards(workshopsCard, applicationRepository);

        var result = await GetWorkshopsWithAverageRating(workshopsCard).ConfigureAwait(false);

        return result;
    }

    private async Task<List<WorkshopCard>> GetWorkshopsWithAverageRating(List<WorkshopCard> workshopsCards)
    {
        var averageRatings = await averageRatingService.GetByEntityIdsAsync(workshopsCards.Select(p => p.Id)).ConfigureAwait(false);

        foreach (var workshop in workshopsCards)
        {
            var averageRatingDto = averageRatings?.SingleOrDefault(r => r.EntityId == workshop.Id);
            workshop.Rating = averageRatingDto?.Rate ?? default;
            workshop.NumberOfRatings = averageRatingDto?.RateQuantity ?? default;
        }

        return workshopsCards;
    }

    public async Task<IEnumerable<CompetitiveEventViewCardDto>> GetPopularCompetitiveEvents(int limit, long catottgId)
    {
        logger.LogInformation("Getting popular competitive events started.");

        var cacheKey = $"GetPopularCompetitions_{limit}_{catottgId}";

        var competitionsResult = await cache.GetOrAddAsync(cacheKey, () =>
            GetPopularCompetitiveEventsFromDatabase(limit, catottgId)).ConfigureAwait(false);

        return competitionsResult;
    }

    public async Task<IEnumerable<CompetitiveEventViewCardDto>> GetPopularCompetitiveEventsFromDatabase(int limit, long catottgId)
    {
        var eventsQuery = competitiveEventRepository
            .Get(whereExpression: e => !e.IsDeleted)
            .Include(e => e.Contacts).ThenInclude(c => c.Address).ThenInclude(a => a.CATOTTG)
            .Include(e => e.OrganizerOfTheEvent)
            .AsQueryable();

        if (catottgId > 0)
        {
            eventsQuery = eventsQuery.Where(e =>
                e.Contacts.Any(c => c.IsDefault &&
                    (c.Address.CATOTTGId == catottgId ||
                     (c.Address.CATOTTG.Category == CodeficatorCategory.CityDistrict.Name &&
                      c.Address.CATOTTG.ParentId == catottgId))));
        }

        var popularEvents = await eventsQuery
            .OrderBy(e => e.ScheduledStartTime)
            .Take(limit)
            .AsNoTracking().ToListAsync();

        logger.LogInformation($"{popularEvents.Count} competitive events were successfully received from DB");

        return popularEvents.ToViewCardDto();
    }
}