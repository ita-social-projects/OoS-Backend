namespace OutOfSchool.Services.Models.WorkshopDrafts;

/// <summary>
///     Will be stored as nested objects in the JSON format of the workshop draft.
///     This entity is specific to the draft and can be hard-deleted if the draft is removed.
/// </summary>
public class WorkshopDescriptionItemDraft
{
    public string SectionName { get; set; }

    public string Description { get; set; }
}