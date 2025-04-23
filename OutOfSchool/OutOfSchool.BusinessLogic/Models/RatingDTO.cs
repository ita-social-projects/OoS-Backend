using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class RatingDto : IDto<Rating, long>
{
    public long Id { get; set; }

    [Range(1, 5)]
    public int Rate { get; set; }

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;

    public string FirstName { get; set; }

    public string LastName { get; set; }
}

public static class RatingDtoExtensions
{
    public static Rating SetToModel(this RatingDto dto, Rating model)
    {
        model.Id = dto.Id;
        model.Rate = dto.Rate;
        model.EntityId = dto.EntityId;
        model.ParentId = dto.ParentId;
        model.CreationTime = dto.CreationTime;
        
        return model;
    }

    public static Rating ToModel(this RatingDto dto)
        => new()
        {
            Id = dto.Id,
            Rate = dto.Rate,
            EntityId = dto.EntityId,
            ParentId = dto.ParentId,
            CreationTime = dto.CreationTime,
        };

    public static RatingDto ToDto(this Rating model)
        => new()
        {
            Id = model.Id,
            Rate = model.Rate,
            EntityId = model.EntityId,
            ParentId = model.ParentId,
            CreationTime = model.CreationTime,
        };

    public static List<RatingDto> ToDto(this IEnumerable<Rating> list)
        => list.MapToList(ToDto);
}