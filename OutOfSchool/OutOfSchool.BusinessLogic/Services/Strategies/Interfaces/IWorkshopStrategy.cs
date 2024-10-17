using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;

public interface IWorkshopStrategy
{
    Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter);
}
