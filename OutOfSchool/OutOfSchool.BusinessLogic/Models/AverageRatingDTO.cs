namespace OutOfSchool.BusinessLogic.Models;

public class AverageRatingDto
{
    public float Rate { get; set; }

    public int RateQuantity { get; set; }

    public Guid EntityId { get; set; }
}

public static class AverageRatingDtoExtensions
{
    public static AverageRatingDto ToDto(this OutOfSchool.Services.Models.AverageRating averageRating)
        => new()
        {
            Rate = (float) averageRating.Rate,
            RateQuantity = averageRating.RateQuantity,
            EntityId = averageRating.EntityId
        };

    public static List<AverageRatingDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.AverageRating> list)
        => list.MapToList(ToDto);
}
