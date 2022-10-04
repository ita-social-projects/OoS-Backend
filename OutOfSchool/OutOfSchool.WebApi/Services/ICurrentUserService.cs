using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Services;

public interface ICurrentUserService
{
    public string UserId { get; }

    public bool IsInRole(Role role);

    public bool IsDeputyOrProviderAdmin();

    public Task UserHasRights(params IUserRights[] userTypes);
}