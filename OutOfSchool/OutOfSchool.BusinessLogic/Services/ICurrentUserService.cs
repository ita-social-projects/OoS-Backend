using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for checking right to access entities in logic.
/// </summary>
public interface ICurrentUserService : ICurrentUser
{
    /// <summary>
    /// Check if user's role is the same as provided.
    /// </summary>
    /// <param name="role">A <see cref="Role"/> to check.</param>
    /// <returns>A <see cref="bool"/> with value true if user's role is the same as provided,
    /// or false otherwise.</returns>
    public bool IsInRole(Role role);

    /// <summary>
    /// Check if user has <see cref="Subrole.ProviderDeputy"/> or <see cref="Subrole.ProviderAdmin"/> subrole.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user has both role <see cref="Role.Provider"/>
    /// and subrole in not <see cref="Subrole.None"/>, false otherwise.</returns>
    public bool IsEmployeeOrProvider();

    /// <summary>
    /// Check if user is an admin.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user is in any admin role, false otherwise.</returns>
    public bool IsAdmin();

    /// <summary>
    /// Check if user is a techadmin.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user is in techadmin role, false otherwise.</returns>
    public bool IsTechAdmin();

    /// <summary>
    /// Check if user is a ministryadmin.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user is in ministryadmin role, false otherwise.</returns>
    public bool IsMinistryAdmin();

    /// <summary>
    /// Check if user is a region admin.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user is in region admin role, false otherwise.</returns>
    public bool IsRegionAdmin();

    /// <summary>
    /// Check if user is an area admin.
    /// </summary>
    /// <returns>A <see cref="bool"/> with value true if user is in area admin role, false otherwise.</returns>
    public bool IsAreaAdmin();

    /// <summary>
    /// Check if user's role is the same as provided.
    /// </summary>
    /// <param name="userTypes">Any number of <see cref="IUserRights"/> that needs to be checked at the execution point.</param>
    /// <returns>Successfully returns from the method if user has any of the specified rights.</returns>
    /// <exception cref="UnauthorizedAccessException">If user has no rights to access data an exception is thrown.</exception>
    public Task UserHasRights(params IUserRights[] userTypes);
}