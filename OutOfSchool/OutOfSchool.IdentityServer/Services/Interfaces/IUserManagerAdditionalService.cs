using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Services.Interfaces
{
    public interface IUserManagerAdditionalService
    {
        Task<IdentityResult> ChangePasswordWithRequiredMustChangePasswordAsync(User user, string currentPassword, string newPassword);
    }
}