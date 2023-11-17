using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ProvidersInfo;

namespace OutOfSchool.WebApi.Services;

public interface IExternalExportProviderService
{
    Task<SearchResult<ProviderInfoBaseDto>> GetProvidersWithWorkshops(DateTime updatedAfter, SizeFilter sizeFilter);
}
