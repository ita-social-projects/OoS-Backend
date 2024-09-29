#nullable enable

using System;

namespace OutOfSchool.Common.Models;

/// <summary>
/// Defines interface for current user.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets current logged in UserId or empty string in other case.
    /// </summary>
    /// <returns><see cref="string"/> id of current logged-in user.</returns>
    public string UserId { get; }

    /// <summary>
    /// Gets current logged in UserRole or empty string in other case.
    /// </summary>
    /// <returns>A <see cref="string"/> id of current logged in user.</returns>
    public string UserRole { get; }

    /// <summary>Determines whether the current user belongs to the specified role.</summary>
    /// <param name="role">The name of the role for which to check membership.</param>
    /// <returns>
    /// <see langword="true" /> if the current user is a member of the specified role; otherwise, <see langword="false" />.</returns>
    bool IsInRole(string role);

    /// <summary>Gets a value indicating whether the user has been authenticated.</summary>
    /// <returns>
    /// <see langword="true" /> if the user was authenticated; otherwise, <see langword="false" />.</returns>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Determines whether the current user has a claim with the specified type and value.
    /// </summary>
    /// <param name="type">The claim type to check for.</param>
    /// <param name="valueComparer">Function to check the claim value. If empty, it only checks if the claim type is present.</param>
    /// <returns>
    /// <see langword="true" /> if the user has the specified claim; otherwise, <see langword="false" />.
    /// </returns>
    bool HasClaim(string type, Func<string, bool>? valueComparer = null);
}