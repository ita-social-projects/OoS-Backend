using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Directions;

public class SubDirectionsInfoDto : SubDirectionsInfoBaseDto
{
    [Required]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    [DataType(DataType.Text)]
    public string Description { get; set; } = string.Empty;

    public long DirectionId { get; set; }
}

public static class SubDirectionsInfoDtoExtensions
{
    public static SubDirectionsInfoBaseDto ToBaseInfoDto(this SubDirection model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
        };

    public static SubDirectionsInfoDto ToInfoDto(this SubDirection model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Title = model.Title,
            Description = model.Description,
            DirectionId = model.DirectionId,
        };

    public static List<SubDirectionsInfoBaseDto> ToBaseOrInfoDto(this IEnumerable<SubDirection> list)
        => list.MapToList(x => x.IsDeleted ? x.ToBaseInfoDto() : x.ToInfoDto());
}