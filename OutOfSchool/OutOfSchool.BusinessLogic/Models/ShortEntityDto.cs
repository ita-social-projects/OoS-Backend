namespace OutOfSchool.BusinessLogic.Models;

public class ShortEntityDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
}

public static class ShortEntityDtoExtensions
{
    public static ShortEntityDto ToShortEntityDto(this Child child)
        => new()
        {
            Id = child.Id,
            Title = child.LastName + " " + child.FirstName + " " + child.MiddleName
        };

    public static List<ShortEntityDto> ToShortEntityDto(this IEnumerable<Child> list)
        => list.MapToList(ToShortEntityDto);

    public static ShortEntityDto ToShortEntityDto(this Workshop child)
        => new()
        {
            Id = child.Id,
            Title = child.Title,
        };

    public static List<ShortEntityDto> ToShortEntityDto(this IEnumerable<Workshop> list)
        => list.MapToList(ToShortEntityDto);
}
