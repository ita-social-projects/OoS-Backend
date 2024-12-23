using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported;

namespace OutOfSchool.BusinessLogic.Services;

public interface IExternalExportService
{
    Task<SearchResult<ProviderInfoBaseDto>> GetProviders(DateTime updatedAfter, OffsetFilter offsetFilter);

    Task<SearchResult<WorkshopInfoBaseDto>> GetWorkshops(DateTime updatedAfter, OffsetFilter offsetFilter);
}
