﻿using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.AuthCommon.Services;

public class ProviderAdminChangesLogService : IProviderAdminChangesLogService
{
    private readonly IEntityAddOnlyRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository;

    public ProviderAdminChangesLogService(IEntityAddOnlyRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository)
    {
        this.providerAdminChangesLogRepository = providerAdminChangesLogRepository;
    }

    public async Task<int> SaveChangesLogAsync(
        ProviderAdmin entity,
        string userId,
        OperationType operationType,
        string propertyName,
        string? oldValue,
        string? newValue)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        var logRecord = new ProviderAdminChangesLog
        {
            ProviderAdminUserId = entity.UserId,
            ProviderId = entity.ProviderId,
            OperationType = operationType,
            OperationDate = DateTime.UtcNow,
            UserId = userId,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            IsDeputy = entity.IsDeputy,
        };

        var result = await providerAdminChangesLogRepository.Create(logRecord);

        return result == null ? 0 : 1;
    }
}