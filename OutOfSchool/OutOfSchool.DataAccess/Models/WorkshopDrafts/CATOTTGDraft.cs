namespace OutOfSchool.Services.Models.WorkshopDrafts;

/// <summary>
///     Will be stored as nested objects in the JSON format of the workshop draft.
///     This entity is specific to the draft and can be hard-deleted if the draft is removed.
/// </summary>
public class CatottgDraft
{
    public string Code { get; set; }

    public long ParentId { get; set; }

    public string Category { get; set; } 

    public string Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public bool NeedCheck { get; set; }

    public int Order { get; set; } 
}
