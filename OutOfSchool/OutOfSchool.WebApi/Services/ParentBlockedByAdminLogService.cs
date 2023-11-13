namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Service for logging parent blocking and unblocking actions performed by administrators.
/// </summary>
public class ParentBlockedByAdminLogService : IParentBlockedByAdminLogService
{
    private readonly IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository;

    public ParentBlockedByAdminLogService(
        IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository)
    {
        this.parentBlockedByAdminLogRepository = parentBlockedByAdminLogRepository;
    }

    /// <summary>
    /// Saves a log record for parent blocking or unblocking actions performed by administrators.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent.</param>
    /// <param name="userId">The unique identifier of the administrator performing the action.</param>
    /// <param name="reason">The reason for blocking or unblocking the parent.</param>
    /// <param name="isBlocked">A boolean indicating whether the parent is being blocked (true) or unblocked (false).</param>
    /// <returns>An asynchronous task representing the result of the operation.
    /// Returns the number of records affected in the log (1 for success, 0 for failure).</returns>
    public async Task<int> SaveChangesLogAsync(Guid parentId, string userId, string reason, bool isBlocked)
    {
        var logRecord = new ParentBlockedByAdminLog
        {
            ParentId = parentId,
            UserId = userId,
            OperationDate = DateTime.Now,
            Reason = reason,
            IsBlocked = isBlocked,
        };

        var result = await parentBlockedByAdminLogRepository.Create(logRecord);
        return result == null ? 1 : 0;
    }
}
