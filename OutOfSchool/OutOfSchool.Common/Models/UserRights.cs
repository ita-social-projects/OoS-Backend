using System;

namespace OutOfSchool.Common.Models;

public interface IUserRights
{
}

public record ParentRights(Guid parentId, Guid childId = default)
    : IUserRights;

public record EmployeeRights(Guid providerId)
    : IUserRights;

public record DeputyDirectorRights(Guid providerId)
    : IUserRights;

public record ProviderRights(Guid providerId)
    : IUserRights;

public record EmployeeWorkshopRights(Guid workshopId = default)
    : IUserRights;

public record TechAdminRights(Guid techAdminId)
    : IUserRights;

public record ModeratorRights(Guid moderatorId)
    : IUserRights;