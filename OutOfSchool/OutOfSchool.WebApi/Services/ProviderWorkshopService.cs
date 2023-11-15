using AutoMapper;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Models;
using OutOfSchool.Services.Models;
using System.Linq.Expressions;

namespace OutOfSchool.WebApi.Services;

public class ProviderWorkshopService: IProviderWorkshopService
{
    private readonly IProviderRepository providerRepository;
    private readonly IWorkshopRepository workshopRepository;
    private readonly IMapper mapper;
    private readonly ILogger<ProviderWorkshopService> logger;

    public ProviderWorkshopService(
        IProviderRepository providerRepository,
        IWorkshopRepository workshopRepository,
        IMapper mapper,
        ILogger<ProviderWorkshopService> logger)
    {
        this.providerRepository = providerRepository;
        this.workshopRepository = workshopRepository;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<SearchResult<ProviderWorkshopDto>> GetProvidersWithWorkshops(DateTime updatedAfter, int pageSize)
    {
        var providers = await GetAllUpdatedProviders(updatedAfter);

        var providerWorkshopDtos = new List<ProviderWorkshopDto>();

        foreach (var provider in providers)
        {
            var workshops = await GetWorkshopListByProviderId(provider.Id);

            var providerDto = mapper.Map<ProviderWorkshopDto>(provider);
            providerDto.Workshops = workshops;
            providerWorkshopDtos.Add(providerDto);
        }

        var paginatedResult = providerWorkshopDtos.Take(pageSize).ToList();
        var searchResult = new SearchResult<ProviderWorkshopDto>
        {
            TotalAmount = providerWorkshopDtos.Count,
            Entities = paginatedResult,
        };

        return searchResult;
    }

    private async Task<List<WorkshopDto>> GetWorkshopListByProviderId(Guid providerId)
    {
        var workshops = await workshopRepository.GetByFilter(
                                             whereExpression: x => x.ProviderId == providerId);

        var workshopsDto = workshops.Select(workshops => mapper.Map<WorkshopDto>(workshops)).ToList();

        return workshopsDto;
    }

    private async Task<List<ProviderDto>> GetAllUpdatedProviders(DateTime updatedAfter)
    {
        var providers = await providerRepository
        .Get(whereExpression: x =>
            (x.UpdatedAt > updatedAfter || (updatedAfter == default && !x.IsDeleted)) ||
                x.Workshops.Any(w => w.UpdatedAt > updatedAfter || (updatedAfter == default && !x.IsDeleted)))
        .ToListAsync()
        .ConfigureAwait(false);

        var providersDto = providers.Select(provider => mapper.Map<ProviderDto>(provider)).ToList();

        return providersDto;
    }
}