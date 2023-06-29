namespace OutOfSchool.WebApi.Models;

public class DirectionFilter : OffsetFilter
{
    public string Name { get; set; } = string.Empty;

    public long? CatottgId { get; set; }
}