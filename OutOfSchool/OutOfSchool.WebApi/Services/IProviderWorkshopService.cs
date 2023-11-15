using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public interface IProviderWorkshopService
{
    Task<SearchResult<ProviderWorkshopDto>> GetProvidersWithWorkshops(DateTime updatedAfter, int pageSize);
}
