using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;

namespace OutOfSchool.BusinessLogic.Services;

public interface IExternalExportService
{
    Task<SearchResult<ProviderInfoBaseDto>> GetProviders(DateTime updatedAfter, OffsetFilter offsetFilter);

    Task<SearchResult<WorkshopInfoBaseDto>> GetWorkshops(DateTime updatedAfter, OffsetFilter offsetFilter);
    
    Task<SearchResult<DirectionInfoBaseDto>> GetDirections(DateTime updatedAfter, OffsetFilter offsetFilter);
    
    Task<SearchResult<SubDirectionsInfoBaseDto>> GetSubDirections(DateTime updatedAfter, OffsetFilter offsetFilter);
}
