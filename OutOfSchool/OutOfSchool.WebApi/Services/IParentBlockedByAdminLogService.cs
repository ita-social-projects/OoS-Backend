namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Service interface for logging parent blocking and unblocking actions performed by administrators.
/// </summary>
public interface IParentBlockedByAdminLogService
{
    /// <summary>
    /// Create changes log for the given ParentId entity.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent entity.</param>
    /// <param name="userId">The unique identifier of the user performing the block or unblock operation.</param>
    /// <param name="reason">The reason for blocking or unblocking the parent.</param>
    /// <param name="isBlocked">A boolean indicating whether the parent is being blocked (true) or unblocked (false).</param>
    /// <returns>Number of the added log records.</returns>
    Task<int> SaveChangesLogAsync(
        Guid parentId,
        string userId,
        string reason,
        bool isBlocked);
}
