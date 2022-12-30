namespace OutOfSchool.WebApi.Models;

public class SearchStringFilter : OffsetFilter
{
    /// <summary>
    /// Gets or sets a value for search.
    /// </summary>
    public string SearchString { get; set; } = string.Empty;
}