using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Services;

public class ProviderAdminChangesLogService : IProviderAdminChangesLogService
{
    private readonly IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository;

    public ProviderAdminChangesLogService(IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository)
    {
        this.providerAdminChangesLogRepository = providerAdminChangesLogRepository;
    }

    public async Task<int> SaveChangesLogAsync(
        ProviderAdmin entity,
        string userId,
        OperationType operationType,
        string propertyName,
        string oldValue,
        string newValue)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        if (entity.ManagedWorkshops?.Count > 0)
        {
            var logRecords = entity.ManagedWorkshops.Select(x => this.CreateChangesLogRecord(
                entity.UserId,
                entity.ProviderId,
                operationType,
                userId,
                propertyName,
                oldValue,
                newValue,
                x.Id));

            var result = await providerAdminChangesLogRepository.Create(logRecords);

            return result.Count();
        }
        else
        {
            var logRecord = this.CreateChangesLogRecord(
                entity.UserId,
                entity.ProviderId,
                operationType,
                userId,
                propertyName,
                oldValue,
                newValue);

            var result = await providerAdminChangesLogRepository.Create(logRecord);

            return result == null ? 0 : 1;
        }
    }

    private ProviderAdminChangesLog CreateChangesLogRecord(
        string providerAdminId,
        Guid providerId,
        OperationType operationType,
        string userId,
        string propertyName,
        string oldValue,
        string newValue,
        Guid? workshopId = null)
        => new ProviderAdminChangesLog
        {
            ProviderAdminUserId = providerAdminId,
            ProviderId = providerId,
            ManagedWorkshopId = workshopId,
            OperationType = operationType,
            OperationDate = DateTime.Now,
            UserId = userId,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
        };
}