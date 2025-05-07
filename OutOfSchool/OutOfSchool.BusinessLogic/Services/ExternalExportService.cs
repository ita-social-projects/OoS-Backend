using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported;
using OutOfSchool.BusinessLogic.Models.Exported.CompetitiveEvents;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class ExternalExportService(
    IProviderRepository providerRepository,
    IWorkshopRepository workshopRepository,
    IApplicationRepository applicationRepository,
    IAverageRatingService averageRatingService,
    IEntityRepositorySoftDeleted<long, Direction> directionRepository,
    ISensitiveEntityRepositorySoftDeleted<CompetitiveEvent> competitiveEventRepository,
    IEntityRepositorySoftDeleted<long, SubDirection> subDirectionRepository,
    ILogger<ExternalExportService> logger
) : IExternalExportService
{
    /// <summary>
    /// Create a delegate to include other entities in Provider entity
    /// </summary>
    private readonly Func<IQueryable<Provider>, IQueryable<Provider>> providerIncludeFunc =
            p => p.Include(p => p.ProviderSectionItems)
                  .Include(p => p.Images)
                  .Include(p => p.Institution)
                  .Include(p => p.Type)
                  .IncludeContactsWithCodeficatorHierarchy();

    /// <summary>
    /// Create a delegate to include other entities in Workshop entity
    /// </summary>
    private readonly Func<IQueryable<Workshop>, IQueryable<Workshop>> workshopIncludeFunc =
            W => W.Include(w => w.WorkshopDescriptionItems)
                  .Include(w => w.Tags)                
                  .Include(w => w.Images)
                  .Include(w => w.DateTimeRanges)
                  .Include(w => w.Teachers)
                  .Include(w => w.InstitutionHierarchy)
                  .ThenInclude(ih => ih.Institution)
                  .Include(w => w.InstitutionHierarchy)
                  .ThenInclude(i => i.SubDirections)
                  .ThenInclude(sb => sb.Direction)
                  .Include(w => w.DefaultTeacher)
                  .IncludeContactsWithCodeficatorHierarchy();

    /// <summary>
    /// Create a delegate to include other entities in CompetitiveEvent entity
    /// </summary>
    private readonly Func<IQueryable<CompetitiveEvent>, IQueryable<CompetitiveEvent>> competitiveEventIncludeFunc =
            ce => ce.Include(ce => ce.CompetitiveEventDescriptionItems)
                    .Include(ce => ce.Parent)
                    .Include(ce => ce.CompetitiveEventAccountingType)
                    .Include(ce => ce.Coverage)
                    .Include(ce => ce.SubDirections)
                    .ThenInclude(s => s.Direction)
                    .IncludeContactsWithCodeficatorHierarchy();

    public async Task<SearchResult<ProviderInfoBaseDto>> GetProviders(DateTime updatedAfter,
        OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogDebug("Getting all updated providers started");
            offsetFilter ??= new OffsetFilter();

            Expression<Func<Provider, bool>> filterExpression = updatedAfter == default
                ? provider => !provider.IsDeleted
                : provider => provider.UpdatedAt > updatedAfter ||
                              provider.Workshops.Any(w => w.UpdatedAt > updatedAfter);

            var providers = await providerRepository.Get(
                    skip: offsetFilter.From,
                    take: offsetFilter.Size,
                    whereExpression: filterExpression)
                .IncludeProperties(providerIncludeFunc)
                .ToListAsync()
                .ConfigureAwait(false);

            var providersDto = providers.ToBaseOrInfoDto();

            await FillRatingsForType(providersDto).ConfigureAwait(false);

            var count = await providerRepository.Count(filterExpression);

            var searchResult = new SearchResult<ProviderInfoBaseDto>
            {
                TotalAmount = count,
                Entities = providersDto,
            };

            return searchResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing providers");
            throw;
        }
    }

    public async Task<SearchResult<WorkshopInfoBaseDto>> GetWorkshops(DateTime updatedAfter, OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogDebug("Getting all updated workshops started");
            offsetFilter ??= new OffsetFilter();

            Expression<Func<Workshop, bool>> filterExpression = updatedAfter == default
                ? workshop => !workshop.IsDeleted
                : workshop => workshop.UpdatedAt > updatedAfter || workshop.DeleteDate > updatedAfter;

            var workshops = await workshopRepository.Get(
                    skip: offsetFilter.From,
                    take: offsetFilter.Size,
                    whereExpression: filterExpression)
                .IncludeProperties(workshopIncludeFunc)
                .ToListAsync()
                .ConfigureAwait(false);

            var workshopsDto = workshops.ToBaseOrInfoDto();

            await FillRatingsForType(workshopsDto).ConfigureAwait(false);
            await FillTakenSeats(workshopsDto).ConfigureAwait(false);

            var count = await workshopRepository.Count(filterExpression).ConfigureAwait(false);

            return new SearchResult<WorkshopInfoBaseDto>
            {
                TotalAmount = count,
                Entities = workshopsDto,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing workshops");
            throw;
        }
    }

    public async Task<SearchResult<CompetitiveEventInfoBaseDto>> GetCompetitiveEvents(DateTime updatedAfter,
        OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogDebug("Getting all updated competitive events started");
            offsetFilter ??= new OffsetFilter();

            Expression<Func<CompetitiveEvent, bool>> filterExpression = updatedAfter == default
                ? competitiveEvent => !competitiveEvent.IsDeleted
                : competitiveEvent => competitiveEvent.UpdatedAt > updatedAfter || competitiveEvent.DeleteDate > updatedAfter;

            var events = await competitiveEventRepository.Get(
                    skip: offsetFilter.From,
                    take: offsetFilter.Size,
                    whereExpression: filterExpression)
                .IncludeProperties(competitiveEventIncludeFunc)
                .ToListAsync()
                .ConfigureAwait(false);

            var eventsDto = events.ToBaseOrInfoDto();

            await FillRatingsForType(eventsDto).ConfigureAwait(false);

            var count = await competitiveEventRepository.Count(filterExpression).ConfigureAwait(false);

            return new SearchResult<CompetitiveEventInfoBaseDto>
            {
                TotalAmount = count,
                Entities = eventsDto,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing competitive events");
            throw;
        }
    }

    public async Task<SearchResult<DirectionInfoBaseDto>> GetDirections(DateTime updatedAfter, OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogDebug("Getting Directions started");

            offsetFilter ??= new OffsetFilter();

            Expression<Func<Direction, bool>> filterExpression = updatedAfter == default
                ? direction => !direction.IsDeleted
                : direction => direction.UpdatedAt > updatedAfter;

            // Is deleted expression is added automatically by repo
            var directions = await directionRepository
                .Get(skip: offsetFilter.From, take: offsetFilter.Size, whereExpression: filterExpression)
                .ToListAsync();

            logger.LogDebug("All {Count} records were successfully received from the Direction table",
                directions.Count);

            // Is deleted expression is added automatically by repo
            var count = await directionRepository.Count(filterExpression).ConfigureAwait(false);

            var result = new SearchResult<DirectionInfoBaseDto>()
            {
                TotalAmount = count,
                Entities = directions.ToBaseOrInfoDto(),
            };

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing directions");
            throw;
        }
    }

    public async Task<SearchResult<SubDirectionsInfoBaseDto>> GetSubDirections(DateTime updatedAfter, OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogDebug("Getting SubDirections started");

            offsetFilter ??= new OffsetFilter();

            Expression<Func<SubDirection, bool>> filterExpression = updatedAfter == default
                ? subDirection => !subDirection.IsDeleted
                : subDirection => subDirection.UpdatedAt > updatedAfter;

            var subDirections = await subDirectionRepository
                .Get(skip: offsetFilter.From, take: offsetFilter.Size, whereExpression: filterExpression)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogDebug("All {Count} records were successfully received from the SubDirection table",
                subDirections.Count);

            var count = await subDirectionRepository.Count(filterExpression).ConfigureAwait(false);

            var result = new SearchResult<SubDirectionsInfoBaseDto>()
            {
                TotalAmount = count,
                Entities = subDirections.ToBaseOrInfoDto(),
            };

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing subdirections");
            throw;
        }
    }

    private async Task FillRatingsForType<T>(List<T> dtos)
        where T : class, IExternalInfo<Guid>
    {
        var ids = dtos.Select(w => w.Id).ToList();

        var averageRatings =
            (await averageRatingService.GetByEntityIdsAsync(ids).ConfigureAwait(false)).ToList();

        foreach (var dto in dtos.OfType<IExternalRatingInfo>())
        {
            var averageRatingsForProvider = averageRatings?.SingleOrDefault(r => r.EntityId == dto.Id);
            dto.Rating = averageRatingsForProvider?.Rate ?? 0;
            dto.NumberOfRatings = averageRatingsForProvider?.RateQuantity ?? 0;
        }
    }

    private async Task FillTakenSeats(List<WorkshopInfoBaseDto> dtos)
    {
        var fullDtos = dtos.OfType<WorkshopInfoDto>().ToList();
        var ids = fullDtos.Select(w => w.Id).ToList();
        var takenSeats = await applicationRepository.CountTakenSeatsForWorkshops(ids).ConfigureAwait(false);
        foreach (var dto in fullDtos)
        {
            var takenSeat = takenSeats?.SingleOrDefault(w => w.WorkshopId == dto.Id)?.TakenSeats;
            dto.TakenSeats = takenSeat ?? 0;
        }
    }
}