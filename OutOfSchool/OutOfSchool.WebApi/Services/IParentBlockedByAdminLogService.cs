namespace OutOfSchool.WebApi.Services;

public interface IParentBlockedByAdminLogService
{
    /// <summary>
    /// Create changes log for the given ParentId entity.
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="userId">ID of user that performs the change.</param>
    /// <param name="reason"></param>
    /// <param name="isBlocked"></param>
    /// <returns>Number of the added log records.</returns>
    Task<int> SaveChangesLogAsync(
        Guid parentId,
        string userId,
        string reason,
        bool isBlocked);
}
