using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Official;

namespace OutOfSchool.BusinessLogic.Services;
public interface IOfficialService
{
    /// <summary>
    /// Gets officials by filter.
    /// </summary>
    /// <param name="providerId">Provider's Id.</param>
    /// <param name="filter">Filter for list of Officials.</param>
    /// <returns>SearchResult that contains a filtered list of Officials and the total amount of officials in the list.</returns>
    Task<SearchResult<OfficialDto>> GetByFilter(Guid providerId, SearchStringFilter filter);
}
