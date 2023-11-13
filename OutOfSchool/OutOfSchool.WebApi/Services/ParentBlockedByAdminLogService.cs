namespace OutOfSchool.WebApi.Services;

public class ParentBlockedByAdminLogService : IParentBlockedByAdminLogService
{
    private readonly IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository;

    public ParentBlockedByAdminLogService(
        IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository)
    {
        this.parentBlockedByAdminLogRepository = parentBlockedByAdminLogRepository;
    }

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
