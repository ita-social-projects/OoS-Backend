namespace OutOfSchool.WebApi.Models;

public class AverageRatingDto
{
    public float Rate { get; set; }

    public int RateQuantity { get; set; }

    public Guid EntityId { get; set; }
}
