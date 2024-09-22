namespace OutOfSchool.Common.Models;

/// <summary>
/// Defines interface for current user id.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets current logged in UserId or empty string in other case.
    /// </summary>
    /// <returns>A <see cref="string"/> id of current logged in user.</returns>
    public string UserId { get; }
}