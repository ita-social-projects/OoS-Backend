namespace OutOfSchool.BusinessLogic.Models.Changes;

public class ParentBlockedByAdminChangesLogDto
{
    public Guid ParentId { get; set; }

    public string ParentFullName { get; set; }

    public ShortUserDto User { get; set; }

    public DateTime OperationDate { get; set; }

    public string Reason { get; set; }

    public bool IsBlocked { get; set; }
}
