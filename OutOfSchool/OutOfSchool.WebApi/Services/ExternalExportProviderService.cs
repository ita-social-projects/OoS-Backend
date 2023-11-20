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

    public async Task<SearchResult<ProviderInfoBaseDto>> GetProvidersWithWorkshops(DateTime updatedAfter, SizeFilter sizeFilter)
    {
        try
        {
            logger.LogInformation("Getting all updated providers started.");

            var providers = await GetAllUpdatedProviders(updatedAfter, sizeFilter);

            if (providers == null)
            {
                logger.LogError("Failed to retrieve updated providers. The provider list is null.");
                return new SearchResult<ProviderInfoBaseDto>();
            }

            var providerWorkshopDtos = new List<ProviderInfoBaseDto>();

            foreach (var provider in providers)
            {
                    var workshops = await GetWorkshopListByProviderId(provider.Id);

                    var providerDto = mapper.Map<ProviderInfoBaseDto>(provider);
                    providerDto.Workshops = workshops;
                    providerWorkshopDtos.Add(providerDto);
            }

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

    private async Task<List<WorkshopInfoBaseDto>> GetWorkshopListByProviderId(Guid providerId)
    {
        var workshops = await workshopRepository.GetAllWithDeleted(
                                             whereExpression: x => x.ProviderId == providerId);

        var workshopsDto = workshops
            .Select(workshop => MapToInfoWorkshopDto(workshop))
            .ToList();

        return workshopsDto;
    }

    private async Task<List<ProviderInfoBaseDto>> GetAllUpdatedProviders(DateTime updatedAfter, SizeFilter sizeFilter)
    {
        sizeFilter ??= new SizeFilter();
        var providers = await providerRepository
         .GetAllWithDeleted()
         .ConfigureAwait(false);

        var filteredProviders = updatedAfter == default
            ? providers.Where(provider => !provider.IsDeleted)
            : providers.Where(provider => IsProviderUpdated(provider, updatedAfter));

        var providersDto = filteredProviders
            .Take(sizeFilter.Size)
            .Select(provider => MapToInfoProviderDto(provider))
            .ToList();

        return providersDto;
    }

    private bool IsProviderUpdated(Provider provider, DateTime updatedAfter)
    {
        return provider.UpdatedAt > updatedAfter ||
                        provider.Workshops.Any(w => w.UpdatedAt > updatedAfter);
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