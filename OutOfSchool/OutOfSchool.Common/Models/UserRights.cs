using System;

namespace OutOfSchool.Common.Models;

public interface IUserRights
{
}

public record ParentRights(Guid parentId, Guid childId = default)
    : IUserRights;

public record ProviderAdminRights(string providerAdminId)
    : IUserRights;

public record ProviderRights(Guid providerId)
    : IUserRights;

public record ProviderAdminWorkshopRights(Guid providerId, Guid workshopId = default)
    : IUserRights;

public record ProviderDeputyRights(Guid providerId)
    : IUserRights;
