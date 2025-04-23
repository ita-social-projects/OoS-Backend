using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class SubDirectionDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }
}

public static class SubDirectionDtoExtensions
{
    public static SubDirection SetToModel(this SubDirectionDto dto, SubDirection model, DateTime updatedAt)
    {
        model.Id = dto.Id;
        model.Title = dto.Title;
        model.Description = dto.Description;
        model.UpdatedAt = updatedAt;

        return model;
    }

    public static SubDirection ToModel(this SubDirectionDto dto, long directionId)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            DirectionId = directionId,
        };

    public static SubDirectionDto ToDto(this SubDirection model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
        };

    public static List<SubDirectionDto> ToDto(this IEnumerable<SubDirection> list)
        => list.MapToList(ToDto);
}
