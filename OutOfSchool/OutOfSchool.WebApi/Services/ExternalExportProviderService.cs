using AutoMapper;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ProvidersInfo;

namespace OutOfSchool.WebApi.Services;

public class ExternalExportProviderService : IExternalExportProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IMapper mapper;
    private readonly ILogger<ExternalExportProviderService> logger;

    public ExternalExportProviderService(
        IProviderRepository providerRepository,
        IWorkshopRepository workshopRepository,
        IMapper mapper,
        ILogger<ExternalExportProviderService> logger)
    {
        this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
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

            var searchResult = new SearchResult<ProviderInfoBaseDto>
            {
                TotalAmount = providerWorkshopDtos.Count,
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
}