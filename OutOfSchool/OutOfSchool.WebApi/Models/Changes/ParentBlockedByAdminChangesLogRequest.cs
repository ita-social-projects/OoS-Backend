namespace OutOfSchool.WebApi.Models.Changes;

public class ParentBlockedByAdminChangesLogRequest : SearchStringFilter
{
    public bool IsBlocked { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}
