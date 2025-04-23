using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models;

public class DirectionDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? WorkshopsCount { get; set; }

    public DirectionDto WithCount(int? count)
    {
        WorkshopsCount = count;
        return this;
    }
}

public static class DirectionDtoExtensions
{
    public static Direction SetToModel(this DirectionDto dto, Direction model)
    {
        model.Id = dto.Id;
        model.Title = dto.Title;
        model.Description = dto.Description;
        
        return model;
    }

    public static Direction ToModel(this DirectionDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
        };

    public static List<Direction> ToModel(this IEnumerable<DirectionDto> list)
        => list.MapToList(ToModel);

    public static DirectionDto ToDto(this Direction model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
        };

    public static DirectionDto ToDto(this Direction model, int? workshopsCount)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            WorkshopsCount = workshopsCount
        };

    public static List<DirectionDto> ToDto(this IEnumerable<Direction> list)
        => list.MapToList(ToDto);
}