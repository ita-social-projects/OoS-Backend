using System;

namespace OutOfSchool.Common.Models;

public interface IUserRights
{
}

public record ParentRights(Guid parentId)
    : IUserRights;

public record ProviderAdminRights(string providerAdminId)
    : IUserRights;

public record ProviderOrAdminRights(Guid providerId, Guid workshopId = default)
    : IUserRights;