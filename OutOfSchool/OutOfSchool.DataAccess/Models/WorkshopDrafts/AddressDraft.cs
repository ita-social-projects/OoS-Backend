
namespace OutOfSchool.Services.Models.WorkshopDrafts;

public class AddressDraft
{
    public string Street { get; set; }

    public string BuildingNumber { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public ulong GeoHash { get; set; } = default;

    public long CATOTTGId { get; set; }
}