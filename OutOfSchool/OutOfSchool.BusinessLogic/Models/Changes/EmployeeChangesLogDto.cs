using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Changes;

public class EmployeeChangesLogDto
{
    public string EmployeeId { get; set; }

    public string EmployeeFullName { get; set; }

    public string ProviderTitle { get; set; }

    public string WorkshopCity { get; set; }

    public OperationType OperationType { get; set; }

    public DateTime OperationDate { get; set; }

    public ShortUserDto User { get; set; }

    public string InstitutionTitle { get; set; }

    public string PropertyName { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }
}

public static class EmployeeChangesLogDtoExtensions
{
    public static EmployeeChangesLogDto ToDto(this EmployeeChangesLog changesLog)
        => new()
        {
            EmployeeId = changesLog.EmployeeUserId,
            EmployeeFullName = $"{changesLog.EmployeeUser.LastName} {changesLog.EmployeeUser.FirstName} {changesLog.EmployeeUser.MiddleName}".TrimEnd(),
            ProviderTitle = changesLog.Provider?.FullTitle,
            WorkshopCity = changesLog.Provider?.Contacts?.SingleOrDefault(c => c.IsDefault)?.Address?.CATOTTG?.Name,
            OperationType = changesLog.OperationType,
            OperationDate = DateTime.SpecifyKind(changesLog.OperationDate, DateTimeKind.Utc),
            User = changesLog.User?.ToShortUser(),
            InstitutionTitle = changesLog.Provider?.Institution?.Title,
            PropertyName = changesLog.PropertyName,
            OldValue = changesLog.OldValue,
            NewValue = changesLog.NewValue,
        };
}