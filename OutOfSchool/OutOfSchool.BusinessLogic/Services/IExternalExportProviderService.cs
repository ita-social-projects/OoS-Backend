using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ProvidersInfo;

namespace OutOfSchool.BusinessLogic.Services;

public interface IExternalExportProviderService
{
    Task<SearchResult<ProviderInfoBaseDto>> GetProvidersWithWorkshops(DateTime updatedAfter, OffsetFilter offsetFilter);
}
