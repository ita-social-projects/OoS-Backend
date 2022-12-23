using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for checking right to access entities in logic.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets current logged in UserId or empty string in other case.
    /// </summary>
    /// <returns>A <see cref="string"/> id of current logged in user.</returns>
    public string UserId { get; }

    /// <summary>
    /// Check if user's role is the same as provided.
    /// </summary>
    /// <param name="role">A <see cref="Role"/> to check.</param>
    /// <returns>A <see cref="bool"/> with value true if user's role is the same as provided,
    /// or false otherwise.</returns>
    public bool IsInRole(Role role);

    /// <summary>
    /// Check if user has <see cref="ProviderSubRole.Deputy"/> or <see cref="ProviderSubRole.Manager"/> subrole.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user has both role <see cref="Role.Provider"/>
    /// and subrole in not <see cref="ProviderSubRole.Provider"/>, false otherwise.</returns>
    public bool IsDeputyOrProviderAdmin();

    /// <summary>
    /// Check if user is an admin.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user is in any admin role, false otherwise.</returns>
    public bool IsAdmin();

    /// <summary>
    /// Check if user's role is the same as provided.
    /// </summary>
    /// <param name="userTypes">Any number of <see cref="IUserRights"/> that needs to be checked at the execution point.</param>
    /// <returns>Successfully returns from the method if user has any of the specified rights.</returns>
    /// <exception cref="UnauthorizedAccessException">If user has no rights to access data an exception is thrown.</exception>
    public Task UserHasRights(params IUserRights[] userTypes);
}