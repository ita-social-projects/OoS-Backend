namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class CompetitiveEventDraftFilterTitle : ExcludeIdFilter
{
    /// <summary>
    /// Gets all entries by entered substring from current competitive event drafts
    /// </summary>
    public string SearchText { get; set; } = string.Empty;
}
