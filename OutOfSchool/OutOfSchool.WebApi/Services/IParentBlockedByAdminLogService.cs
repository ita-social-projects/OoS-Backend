namespace OutOfSchool.WebApi.Services;

public interface IParentBlockedByAdminLogService
{
    /// <summary>
    /// Create changes log for the given ParentId entity.
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="userId">ID of user that performs the change.</param>
    /// <param name="reason"></param>
    /// <param name="oldValue">Old value of the property that is changing.</param>
    /// <param name="newValue">New value of the property that is changing.</param>
    /// <returns>Number of the added log records.</returns>
    Task<int> SaveChangesLogAsync(
        Guid parentId,
        string userId,
        string reason,
        string oldValue,
        string newValue);
}
