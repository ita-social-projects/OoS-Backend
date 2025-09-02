namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
public class CompetitiveEventFilterTitle : ExcludeIdFilter
{
    /// <summary>
    /// Gets all entries by entered substring from current competitive events
    /// </summary>
    public string SearchText { get; set; } = string.Empty;
}
