using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

public class FavoriteDto
{
    public long Id { get; set; }

    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public string UserId { get; set; }
}

public static class FavoriteDtoExtensions
{
    public static Favorite SetToModel(this FavoriteDto dto, Favorite model)
    {
        model.WorkshopId = dto.WorkshopId;
        model.UserId = dto.UserId;
        
        return model;
    }

    public static Favorite ToModel(this FavoriteDto dto)
        => new()
        {
            Id = dto.Id,
            WorkshopId = dto.WorkshopId,
            UserId = dto.UserId,
        };

    public static List<Favorite> ToModel(this IEnumerable<FavoriteDto> list)
        => list.MapToList(ToModel);

    public static FavoriteDto ToDto(this Favorite model)
        => new()
        {
            Id = model.Id,
            WorkshopId = model.WorkshopId,
            UserId = model.UserId,
        };

    public static List<FavoriteDto> ToDto(this IEnumerable<Favorite> list)
        => list.MapToList(ToDto);
}