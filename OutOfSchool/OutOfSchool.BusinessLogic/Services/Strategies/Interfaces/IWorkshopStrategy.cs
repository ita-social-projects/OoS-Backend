using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops.Cards;
using OutOfSchool.BusinessLogic.Models.Workshops.Filters;

namespace OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;

public interface IWorkshopStrategy
{
    Task<SearchResult<WorkshopCard>> SearchAsync(WorkshopFilter filter);
}
