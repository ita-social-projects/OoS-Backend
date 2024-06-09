using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Extensions;
public static class SearchResultExtensions
{
    /// <summary>
    /// Method, that checks SearchResult for null and his entities for empty.
    /// </summary>
    /// <typeparam name="T">Typeof entity.</typeparam>
    /// <param name="searchResult">Instance of SearchResult.</param>
    /// <returns>
    /// True - searchResult is null or searchResult.Entities is empty.
    /// False - searchResult is not null or searchResult.Entities is not empty.
    /// </returns>
    public static bool IsNullOrEntitiesEmpty<T>(this SearchResult<T> searchResult)
    {
        return searchResult == null || searchResult.Entities.Count == 0;
    }
}
