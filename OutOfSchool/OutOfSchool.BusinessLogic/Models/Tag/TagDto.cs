namespace OutOfSchool.BusinessLogic.Models.Tag;

public class TagDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public static class TagDtoExtensions
{
    public static TagDto ToDto(this OutOfSchool.Services.Models.Tag model)
        => new()
        {
            Id = model.Id,
            Name = model.Name,
        };

    public static List<TagDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.Tag> list)
        => list.MapToList(ToDto);
}
