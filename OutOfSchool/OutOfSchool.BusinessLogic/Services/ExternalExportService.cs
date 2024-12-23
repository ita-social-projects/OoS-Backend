using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class ExternalExportService : IExternalExportService
{
    private readonly IProviderRepository providerRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IAverageRatingService averageRatingService;
    private readonly IMapper mapper;
    private readonly ILogger<ExternalExportService> logger;

    public ExternalExportService(
        IProviderRepository providerRepository,
        IWorkshopRepository workshopRepository,
        IAverageRatingService averageRatingService,
        IMapper mapper,
        ILogger<ExternalExportService> logger)
    {
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
        this.averageRatingService =
            averageRatingService ?? throw new ArgumentNullException(nameof(averageRatingService));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SearchResult<ProviderInfoBaseDto>> GetProviders(DateTime updatedAfter,
        OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogInformation("Getting all updated providers started");
            offsetFilter ??= new OffsetFilter();
            var providers = await providerRepository
                .GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size)
                .ConfigureAwait(false);

            if (providers == null)
            {
                logger.LogError("Failed to retrieve updated providers. The provider list is null");
                return new SearchResult<ProviderInfoBaseDto>();
            }

            var providersDto = providers
                .Select(MapToInfoProviderDto)
                .ToList();

            await FillRatingsForType(providersDto).ConfigureAwait(false);

            var count = await providerRepository.CountWithDeleted(updatedAfter);

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
            return new SearchResult<ProviderInfoBaseDto>();
        }
    }

    public async Task<SearchResult<WorkshopInfoBaseDto>> GetWorkshops(DateTime updatedAfter, OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogInformation("Getting all updated providers started");
            offsetFilter ??= new OffsetFilter();

            var workshops = await workshopRepository
                .GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size)
                .ConfigureAwait(false);

            if (workshops == null)
            {
                logger.LogError("Failed to retrieve updated workshops. The workshop list is null");
                return new SearchResult<WorkshopInfoBaseDto>();
            }

            var workshopsDto = workshops.Select(MapToInfoWorkshopDto).ToList();

            await FillRatingsForType(workshopsDto);

            var count = await workshopRepository.CountWithDeleted(updatedAfter);

            return new SearchResult<WorkshopInfoBaseDto>
            {
                TotalAmount = count,
                Entities = workshopsDto,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing workshops");
            return new SearchResult<WorkshopInfoBaseDto>();
        }
    }

    private ProviderInfoBaseDto MapToInfoProviderDto(Provider provider)
    {
        return provider.IsDeleted
            ? mapper.Map<ProviderInfoBaseDto>(provider)
            : mapper.Map<ProviderInfoDto>(provider);
    }

    private WorkshopInfoBaseDto MapToInfoWorkshopDto(Workshop workshop)
    {
        return workshop.IsDeleted
            ? mapper.Map<WorkshopInfoBaseDto>(workshop)
            : mapper.Map<WorkshopInfoDto>(workshop);
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
}