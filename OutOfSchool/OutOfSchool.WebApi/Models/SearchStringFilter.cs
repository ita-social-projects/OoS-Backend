namespace OutOfSchool.WebApi.Models;

public class SearchStringFilter : OffsetFilter
{
    public string SearchString { get; set; } = string.Empty;
}