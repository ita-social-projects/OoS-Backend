using System;

namespace OutOfSchool.Common.Models;

// TODO: Naming
// TODO: move to common to use in every project?
public interface IUserType
{
}

public record ParentType(Guid parentId)
    : IUserType;

public record ProviderType(string providerAdminId)
    : IUserType;

public record ProviderAdminType(Guid providerId, Guid workshopId = default)
    : IUserType;