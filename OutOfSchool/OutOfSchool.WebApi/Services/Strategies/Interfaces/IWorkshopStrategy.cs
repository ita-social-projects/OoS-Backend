using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;

namespace OutOfSchool.WebApi.Services.Strategies.Interfaces;

public interface IWorkshopStrategy
{
    Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter);
}
