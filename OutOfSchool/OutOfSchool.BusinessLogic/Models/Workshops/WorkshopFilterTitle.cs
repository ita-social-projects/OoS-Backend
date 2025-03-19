namespace OutOfSchool.BusinessLogic.Models.Workshops;
public class WorkshopFilterTitle : ExcludeIdFilter
{
    /// <summary>
    /// Gets all entries by entered substring from current workshops
    /// </summary>
    public string SearchText { get; set; } = string.Empty;
}
