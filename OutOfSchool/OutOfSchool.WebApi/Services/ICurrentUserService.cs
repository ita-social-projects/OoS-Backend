using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Services;

public interface ICurrentUserService
{
    public string UserId { get; }

    public bool IsInRole(Role role);

    public Task UserHasRights(params IUserType[] userTypes);
}