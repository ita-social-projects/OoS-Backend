using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services.Strategies.Interfaces;

public interface IWorkshopStrategy
{
    Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter);
}
