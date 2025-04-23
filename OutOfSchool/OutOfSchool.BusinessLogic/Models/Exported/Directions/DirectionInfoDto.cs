using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Directions;

public class DirectionInfoDto : DirectionInfoBaseDto
{
    [Required]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    [DataType(DataType.Text)]
    public string Description { get; set; } = string.Empty;
}

public static class DirectionInfoDtoExtensions
{
    public static DirectionInfoBaseDto ToBaseInfoDto(this Direction model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
        };

    public static DirectionInfoDto ToInfoDto(this Direction model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Title = model.Title,
            Description = model.Description,
        };

    public static List<DirectionInfoBaseDto> ToBaseOrInfoDto(this IEnumerable<Direction> list)
        => list.MapToList(x => x.IsDeleted ? x.ToBaseInfoDto() : x.ToInfoDto());
}