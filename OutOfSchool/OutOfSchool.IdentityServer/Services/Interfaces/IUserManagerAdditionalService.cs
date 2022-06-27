using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Services.Interfaces
{
    /// <summary>
    /// Contains additional methods for UserManager, including operations with transactions.
    /// </summary>
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
}