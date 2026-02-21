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

public static class ParentBlockedByAdminChangesLogDtoExtensions
{
    public static ParentBlockedByAdminChangesLogDto ToDto(this ParentBlockedByAdminLog log)
        => new()
        {
            ParentId = log.ParentId,
            ParentFullName = log.Parent?.User is null 
                ? default 
                : $"{log.Parent.User.LastName} {log.Parent.User.FirstName} {log.Parent.User.MiddleName}".TrimEnd(),
            User = log.User?.ToShortUser(),
            OperationDate = DateTime.SpecifyKind(log.OperationDate, DateTimeKind.Utc),
            Reason = log.Reason,
            IsBlocked = log.IsBlocked,
        };

    public static List<ParentBlockedByAdminChangesLogDto> ToDto(this IEnumerable<ParentBlockedByAdminLog> list)
        => list.MapToList(ToDto);
}
