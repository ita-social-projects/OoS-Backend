using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Extensions;
public static class SearchResultExtensions
{
    public static bool IsNullOrEmpty<T>(this SearchResult<T> searchResult)
        => searchResult == null || searchResult.TotalAmount == 0;
}
