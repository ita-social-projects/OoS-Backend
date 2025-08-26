namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftFilterTitle : ExcludeIdFilter
{
    /// <summary>
    /// Gets all entries by entered substring from current workshop drafts
    /// </summary>
    public string SearchText { get; set; } = string.Empty;
}
