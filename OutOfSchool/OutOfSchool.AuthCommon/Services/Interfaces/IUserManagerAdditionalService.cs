namespace OutOfSchool.AuthCommon.Services.Interfaces;

/// <summary>
/// Contains additional methods for UserManager, including operations with transactions.
/// </summary>
[Obsolete("Change password API is no longer supported. Exists only for testing purposes.")]
public interface IUserManagerAdditionalService
{
    /// <summary>
    /// Changes Password and sets MustChangePassword to false if succeeded.
    /// </summary>
    /// <param name="user">User.</param>
    /// <param name="currentPassword">Current password for user.</param>
    /// <param name="newPassword">New password for user.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation, containing <see cref="IdentityResult"/>.</returns>
    Task<IdentityResult> ChangePasswordWithRequiredMustChangePasswordAsync(User user, string currentPassword, string newPassword);
}