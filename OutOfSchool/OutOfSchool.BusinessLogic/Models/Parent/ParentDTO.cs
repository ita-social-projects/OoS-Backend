namespace OutOfSchool.BusinessLogic.Models;

public class ParentDTO
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;
}

public static class ParentDTOExtensions
{
    public static ParentDTO ToDto(this OutOfSchool.Services.Models.Parent model)
        => new()
        {
            Id = model.Id,
            UserId = model.UserId,
        };

    public static List<ParentDTO> ToDto(this IEnumerable<OutOfSchool.Services.Models.Parent> list)
        => list.MapToList(ToDto);
}