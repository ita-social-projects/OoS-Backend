namespace OutOfSchool.Services.Models.WorkshopDrafts;

/// <summary>
///     Will be stored as nested objects in the JSON format of the workshop draft.
///     This entity is specific to the draft and can be hard-deleted if the draft is removed.
/// </summary>
public class AddressDraft
{
    public string Street { get; set; }

    public string BuildingNumber { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public long CATOTTGId { get; set; }
}