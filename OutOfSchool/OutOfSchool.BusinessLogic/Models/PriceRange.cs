namespace OutOfSchool.BusinessLogic.Models;
public class PriceRange
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

public static class PriceRangeExtensions
{
    public static PriceRange ToDto(this PriceRangeES dto)
        => new()
        {
            MinPrice = dto.MinPrice,
            MaxPrice = dto.MaxPrice,
        };
}