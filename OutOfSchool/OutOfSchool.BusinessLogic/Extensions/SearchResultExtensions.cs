using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Extensions;
public static class SearchResultExtensions
{
    public static bool IsNullOrEntitiesEmpty<T>(this SearchResult<T> searchResult)
        => searchResult == null || searchResult.Entities.Count == 0;
}
