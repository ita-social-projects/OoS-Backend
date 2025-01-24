using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class ExternalExportService : IExternalExportService
{
    private const string ProviderIncludes =
        "ProviderSectionItems,Images,Institution,ActualAddress,ActualAddress.CATOTTG.Parent.Parent.Parent.Parent,LegalAddress,LegalAddress.CATOTTG.Parent.Parent.Parent.Parent,Type";

    private const string WorkshopIncludes =
        "WorkshopDescriptionItems,Tags,Address,Address.CATOTTG.Parent.Parent.Parent.Parent,Images,DateTimeRanges,Teachers,InstitutionHierarchy,InstitutionHierarchy.Institution,InstitutionHierarchy.Directions,DefaultTeacher";

    private readonly IProviderRepository providerRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IApplicationRepository applicationRepository;
    private readonly IAverageRatingService averageRatingService;
    private readonly IEntityRepositorySoftDeleted<long, Direction> directionRepository;
    private readonly ISensitiveEntityRepositorySoftDeleted<Institution> institutionRepository;
    private readonly IInstitutionHierarchyRepository institutionHierarchyRepository;
    private readonly IMapper mapper;
    private readonly ILogger<ExternalExportService> logger;

    public ExternalExportService(
        IProviderRepository providerRepository,
        IWorkshopRepository workshopRepository,
        IApplicationRepository applicationRepository,
        IAverageRatingService averageRatingService,
        IEntityRepositorySoftDeleted<long, Direction> directionRepository,
        ISensitiveEntityRepositorySoftDeleted<Institution> institutionRepository,
        IInstitutionHierarchyRepository institutionHierarchyRepository,
        IMapper mapper,
        ILogger<ExternalExportService> logger)
    {
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
        this.applicationRepository =
            applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        this.averageRatingService =
            averageRatingService ?? throw new ArgumentNullException(nameof(averageRatingService));
        this.directionRepository = directionRepository ?? throw new ArgumentNullException(nameof(directionRepository));
        this.institutionRepository =
            institutionRepository ?? throw new ArgumentNullException(nameof(institutionRepository));
        this.institutionHierarchyRepository = institutionHierarchyRepository ??
                                              throw new ArgumentNullException(nameof(institutionHierarchyRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
                    offsetFilter.From,
                    offsetFilter.Size,
                    ProviderIncludes,
                    filterExpression)
                .ToListAsync()
                .ConfigureAwait(false);

            var providersDto = providers
                .Select(MapToInfoDto<Provider, ProviderInfoBaseDto, ProviderInfoDto>)
                .ToList();

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
            logger.LogDebug("Getting all updated providers started");
            offsetFilter ??= new OffsetFilter();

            Expression<Func<Workshop, bool>> filterExpression = updatedAfter == default
                ? workshop => !workshop.IsDeleted
                : workshop => workshop.UpdatedAt > updatedAfter || workshop.DeleteDate > updatedAfter;

            var workshops = await workshopRepository.Get(
                    offsetFilter.From,
                    offsetFilter.Size,
                    WorkshopIncludes,
                    filterExpression)
                .ToListAsync()
                .ConfigureAwait(false);

            var workshopsDto = workshops
                .Select(MapToInfoDto<Workshop, WorkshopInfoBaseDto, WorkshopInfoDto>)
                .ToList();

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

            var directionDtos = directions
                .Select(MapToInfoDto<Direction, DirectionInfoBaseDto, DirectionInfoDto>)
                .ToList();

            var result = new SearchResult<DirectionInfoBaseDto>()
            {
                TotalAmount = count,
                Entities = directionDtos,
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
            
            Expression<Func<Institution, bool>> institutionFilterExpression = updatedAfter == default
                ? institution => !institution.IsDeleted
                : institution => institution.UpdatedAt > updatedAfter;
            
            Expression<Func<InstitutionHierarchy, bool>> institutionHierarchyFilterExpression = updatedAfter == default
                ? ih => !ih.IsDeleted
                : ih => ih.UpdatedAt > updatedAfter;

            // Is deleted expression is added automatically by repo
            var institutions = institutionRepository
                .Get(whereExpression: institutionFilterExpression);

            // We need to return only the lowest level for each institution
            // Is deleted expression is added automatically by repo
            var institutionSubDirections = institutionHierarchyRepository
                .Get(whereExpression: institutionHierarchyFilterExpression)
                .Join(
                    institutions,
                    ih => new {IId = ih.InstitutionId, Level = ih.HierarchyLevel},
                    i => new {IId = i.Id, Level = i.NumberOfHierarchyLevels},
                    (ih, i) => ih);

            // Is deleted expression is added automatically by repo
            var count = await institutionSubDirections.CountAsync().ConfigureAwait(false);

            if (offsetFilter.From > 0)
            {
                institutionSubDirections = institutionSubDirections.Skip(offsetFilter.From);
            }

            if (offsetFilter.Size > 0)
            {
                institutionSubDirections = institutionSubDirections.Take(offsetFilter.Size);
            }

            var subDirections = await institutionSubDirections
                .IncludeProperties("Directions")
                .OrderBy(ih => ih.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var subDirectionDtos = subDirections
                .Select(MapToInfoDto<InstitutionHierarchy, SubDirectionsInfoBaseDto, SubDirectionsInfoDto>)
                .ToList();

            var result = new SearchResult<SubDirectionsInfoBaseDto>()
            {
                TotalAmount = count,
                Entities = subDirectionDtos,
            };

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing subdirections");
            throw;
        }
    }

    private TBase MapToInfoDto<TEntity, TBase, TFull>(TEntity entity)
        where TEntity : ISoftDeleted
        where TFull : TBase => entity.IsDeleted ? mapper.Map<TBase>(entity) : mapper.Map<TFull>(entity);

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