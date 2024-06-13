using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Extensions;
public static class SearchResultExtensions
{
    public static bool IsNullOrEntitiesEmpty<T>(this SearchResult<T> searchResult)
        => IsNull(searchResult) || searchResult.Entities is null || searchResult.Entities.Count == 0;

    public static bool IsNullOrTotalAmountIsZero<T>(this SearchResult<T> searchResult)
        => IsNull(searchResult) || searchResult.TotalAmount == 0;

    private static bool IsNull<T>(SearchResult<T> searchResult) => searchResult == null;
}
