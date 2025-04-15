using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class OfficialChangesLogService : IOfficialChangesLogService
{
    private readonly IEntityAddOnlyRepository<long, EmployeeChangesLog> employeeChangesLogRepository;

    public OfficialChangesLogService(IEntityAddOnlyRepository<long, EmployeeChangesLog> employeeChangesLogRepository)
    {
        this.employeeChangesLogRepository = employeeChangesLogRepository;
    }

    public async Task<int> SaveChangesLogAsync(
        Official entity,
        string userId,
        OperationType operationType,
        string propertyName,
        string? oldValue,
        string? newValue)
    {
        _ = entity ?? throw new ArgumentNullException(nameof(entity));

        var logRecord = new EmployeeChangesLog
        {
            EmployeeUserId = entity.Individual.UserId,
            ProviderId = entity.Position.ProviderId,
            OperationType = operationType,
            OperationDate = DateTime.UtcNow,
            UserId = userId,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
        };

        var result = await employeeChangesLogRepository.Create(logRecord);

        return result == null ? 0 : 1;
    }
}