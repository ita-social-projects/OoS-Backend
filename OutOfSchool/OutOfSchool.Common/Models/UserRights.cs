using System;

namespace OutOfSchool.Common.Models;

public interface IUserRights
{
}

public record ParentRights(Guid parentId, Guid childId = default)
    : IUserRights;

public record EmployeeRights(string employeeId)
    : IUserRights;

public record ProviderRights(Guid providerId)
    : IUserRights;

public record EmployeeWorkshopRights(Guid providerId, Guid workshopId = default)
    : IUserRights;
