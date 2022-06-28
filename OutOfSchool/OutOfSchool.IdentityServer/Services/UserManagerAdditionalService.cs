using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using OutOfSchool.IdentityServer.Services.Interfaces;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Services
{
    public class UserManagerAdditionalService : IUserManagerAdditionalService
    {
        private readonly ILogger<UserManagerAdditionalService> logger;
        private readonly UserManager<User> userManager;
        private readonly OutOfSchoolDbContext storeContext;

        public UserManagerAdditionalService(
            ILogger<UserManagerAdditionalService> logger,
            UserManager<User> userManager,
            OutOfSchoolDbContext storeContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.storeContext = storeContext ?? throw new ArgumentNullException(nameof(storeContext));
        }

        public async Task<IdentityResult> ChangePasswordWithRequiredMustChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            _ = currentPassword ?? throw new ArgumentNullException(nameof(currentPassword));
            _ = newPassword ?? throw new ArgumentNullException(nameof(newPassword));

            var executionStrategy = storeContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(
                async () =>
                {
                    await using IDbContextTransaction transaction = await storeContext.Database.BeginTransactionAsync();
                    try
                    {
                        logger.LogTrace("ChangePasswordWithRequiredMustChangePassword was started");

                        var identityResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                        if (!identityResult.Succeeded)
                        {
                            logger.LogError("Password wasn't changed for user with email {Email}", user.Email);
                            await transaction.RollbackAsync().ConfigureAwait(false);
                            return identityResult;
                        }

                        user.MustChangePassword = false;
                        identityResult = await userManager.UpdateAsync(user);
                        if (!identityResult.Succeeded)
                        {
                            logger.LogError(
                                "MustChangePassword indicator wasn't changed for user with email {Email}",
                                user.Email);
                            await transaction.RollbackAsync().ConfigureAwait(false);
                            return identityResult;
                        }

                        await transaction.CommitAsync().ConfigureAwait(false);
                        return identityResult;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ChangePasswordWithRequiredMustChangePassword failed");
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        return IdentityResult.Failed();
                    }
                    finally
                    {
                        logger.LogTrace("ChangePasswordWithRequiredMustChangePassword was finished");
                    }
                });
        }
    }
}