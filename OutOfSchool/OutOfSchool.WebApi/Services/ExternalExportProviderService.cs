using AutoMapper;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ProvidersInfo;
using OutOfSchool.WebApi.Services.AverageRatings;

namespace OutOfSchool.WebApi.Services;

public class ExternalExportProviderService : IExternalExportProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IAverageRatingService averageRatingService;
    private readonly IMapper mapper;
    private readonly ILogger<ExternalExportProviderService> logger;

    public ExternalExportProviderService(
        IProviderRepository providerRepository,
        IWorkshopRepository workshopRepository,
        IAverageRatingService averageRatingService,
        IMapper mapper,
        ILogger<ExternalExportProviderService> logger)
    {
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
        this.averageRatingService = averageRatingService ?? throw new ArgumentNullException(nameof(averageRatingService));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SearchResult<ProviderInfoBaseDto>> GetProvidersWithWorkshops(DateTime updatedAfter, OffsetFilter offsetFilter)
    {
        try
        {
            logger.LogInformation("Getting all updated providers started.");
            var providers = await GetAllUpdatedProviders(updatedAfter, offsetFilter);

            if (providers == null)
            {
                logger.LogError("Failed to retrieve updated providers. The provider list is null.");
                return new SearchResult<ProviderInfoBaseDto>();
            }

            var providerIds = providers.Select(provider => provider.Id).ToList();

            var providerWorkshopsMap = await GetWorkshopListByProviderIds(providerIds);

            var providerWorkshopDtos = providers.Select(provider =>
            {
                var providerDto = mapper.Map<ProviderInfoBaseDto>(provider);
                providerDto.Workshops = providerWorkshopsMap.TryGetValue(provider.Id, out var workshops)
                    ? workshops
                    : new List<WorkshopInfoBaseDto>();
                return providerDto;
            }).ToList();

            logger.LogInformation(!providerWorkshopDtos.Any()
                ? "Providers table is empty."
                : $"All {providerWorkshopDtos.Count} records were successfully received from the Provider table");

            int count = providerRepository.CountWithDeleted();

            var searchResult = new SearchResult<ProviderInfoBaseDto>
            {
                TotalAmount = count,
                Entities = providerWorkshopDtos,
            };

            return searchResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while processing providers.");
            return new SearchResult<ProviderInfoBaseDto>();
        }
    }

    private async Task<Dictionary<Guid, List<WorkshopInfoBaseDto>>> GetWorkshopListByProviderIds(List<Guid> providerIds)
    {
        var workshops = await workshopRepository.GetAllWithDeleted(
                                     whereExpression: x => providerIds.Contains(x.ProviderId))
                                     .ConfigureAwait(false);

        var workshopsDto = workshops
         .GroupBy(w => w.ProviderId)
         .ToDictionary(group => group.Key, group => group.Select(workshop => MapToInfoWorkshopDto(workshop)).ToList());

        await FillRatingsForWorkshops(workshopsDto);

        return workshopsDto;
    }

    private async Task<List<ProviderInfoBaseDto>> GetAllUpdatedProviders(DateTime updatedAfter, OffsetFilter offsetFilter)
    {
        offsetFilter ??= new OffsetFilter();

        var providers = await providerRepository
                        .GetAllWithDeleted(updatedAfter, offsetFilter.From, offsetFilter.Size)
                        .ConfigureAwait(false);

        var providersDto = providers
            .Select(provider => MapToInfoProviderDto(provider))
            .ToList();

        await FillRatingsForProviders(providersDto).ConfigureAwait(false);

        return providersDto;
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

    private async Task FillRatingsForProviders(List<ProviderInfoBaseDto> providersDTO)
    {
        var providerIds = providersDTO.Select(p => p.Id).ToList();
        var averageRatings = await averageRatingService.GetByEntityIdsAsync(providerIds).ConfigureAwait(false);

        foreach (var providerDto in providersDTO.OfType<ProviderInfoDto>())
        {
                var averageRatingsForProvider = averageRatings?.SingleOrDefault(r => r.EntityId == providerDto.Id);
                providerDto.Rating = averageRatingsForProvider?.Rate ?? default;
                providerDto.NumberOfRatings = averageRatingsForProvider?.RateQuantity ?? default;
        }
    }

    private async Task FillRatingsForWorkshops(Dictionary<Guid, List<WorkshopInfoBaseDto>> workshopsDtoMap)
    {
        var workshopIds = workshopsDtoMap.SelectMany(kv => kv.Value.Select(w => w.Id)).ToList();
        var averageRatings = await averageRatingService.GetByEntityIdsAsync(workshopIds).ConfigureAwait(false);

        foreach (var (providerId, workshopsDto) in workshopsDtoMap)
        {
            foreach (var workshopDto in workshopsDto.OfType<WorkshopInfoDto>())
            {
                    var averageRatingsForWorkshop = averageRatings?.SingleOrDefault(r => r.EntityId == workshopDto.Id);
                    workshopDto.Rating = averageRatingsForWorkshop?.Rate ?? default;
                    workshopDto.NumberOfRatings = averageRatingsForWorkshop?.RateQuantity ?? default;
            }
        }
    }
}