using System;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.IdentityServer.Services.Interfaces;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.IdentityServer.Services;

public class ProviderAdminChangesLogService : IProviderAdminChangesLogService
{
    private readonly IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository;

    public ProviderAdminChangesLogService(IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository)
    {
        this.providerAdminChangesLogRepository = providerAdminChangesLogRepository;
    }

    public async Task<int> SaveChangesLogAsync(ProviderAdmin entity, string userId, OperationType operationType)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        if (entity.ManagedWorkshops?.Count > 0)
        {
            var logRecords = entity.ManagedWorkshops.Select(x => CreateChangesLogRecord(
                entity.UserId,
                entity.ProviderId,
                operationType,
                userId,
                x.Id));

            var result = await providerAdminChangesLogRepository.Create(logRecords);

            return result.Count();
        }
        else
        {
            var logRecord = CreateChangesLogRecord(
                entity.UserId,
                entity.ProviderId,
                operationType,
                userId);

            var result = await providerAdminChangesLogRepository.Create(logRecord);

            return result == null ? 0 : 1;
        }
    }

    private ProviderAdminChangesLog CreateChangesLogRecord(
        string providerAdminId,
        Guid providerId,
        OperationType operationType,
        string userId,
        Guid? workshopId = null)
        => new ProviderAdminChangesLog
        {
            ProviderAdminUserId = providerAdminId,
            ProviderId = providerId,
            ManagedWorkshopId = workshopId,
            OperationType = operationType,
            OperationDate = DateTime.Now,
            UserId = userId,
        };
}