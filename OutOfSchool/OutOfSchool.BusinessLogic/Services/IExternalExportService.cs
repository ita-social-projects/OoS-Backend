using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;

namespace OutOfSchool.BusinessLogic.Services;

public interface IExternalExportService
{
    Task<SearchResult<ProviderInfoBaseDto>> GetProviders(DateTime updatedAfter, OffsetFilter offsetFilter);

    Task<SearchResult<WorkshopInfoBaseDto>> GetWorkshops(DateTime updatedAfter, OffsetFilter offsetFilter);
    
    Task<SearchResult<DirectionInfoDto>> GetDirections(OffsetFilter offsetFilter);
    
    Task<SearchResult<SubDirectionsInfoDto>> GetSubDirections(OffsetFilter offsetFilter);
}
