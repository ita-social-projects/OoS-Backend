using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models.Changes;

public class ParentBlockedByAdminChangesLogRequest : SearchStringFilter
{
    public ShowParents ShowParents { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}
