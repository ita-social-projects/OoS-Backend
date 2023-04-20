using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OutOfSchool.AdminInitializer.Config;
using OutOfSchool.Common;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.AdminInitializer;

internal class AdminInitializer
{
    private readonly AdminConfiguration adminConfiguration;
    private readonly UserManager<User> userManager;
    private readonly OutOfSchoolDbContext dbContext;
    private readonly int maxRetryCount = 5;
    private readonly int checkConnectivityDelay = 5000;
    private int retry = 0;

    public AdminInitializer(
        IOptions<AdminConfiguration> adminConfiguration,
        UserManager<User> userManager,
        OutOfSchoolDbContext dbContext)
    {
        this.adminConfiguration = adminConfiguration.Value;
        this.userManager = userManager;
        this.dbContext = dbContext;
    }

    public async Task<int> InitAdminUser()
    {
        var user = new User
        {
            UserName = adminConfiguration.Email,
            FirstName = adminConfiguration.FirstName,
            LastName = adminConfiguration.LastName,
            MiddleName = adminConfiguration.MiddleName,
            Email = adminConfiguration.Email,
            PhoneNumber = Constants.PhonePrefix + adminConfiguration.PhoneNumber,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = adminConfiguration.Role,
            IsRegistered = true,
            IsBlocked = false,
            EmailConfirmed = true,
        };

        while (!dbContext.Database.CanConnect())
        {
            if (retry == maxRetryCount)
            {
                return 1;
            }

            Console.WriteLine($"Can't connect to database. Attempt {retry + 1}");
            await Task.Delay(checkConnectivityDelay);
            retry++;
        }

        try
        {
            var existingUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Role == user.Role);
            if (existingUser != null && !adminConfiguration.Reset)
            {
                return 0;
            }

            IdentityResult result;
            if (existingUser != null && adminConfiguration.Reset)
            {
                existingUser.UserName = user.UserName;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.MiddleName = user.MiddleName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.PasswordHash = userManager.PasswordHasher.HashPassword(existingUser, adminConfiguration.Password);
                result = await userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    return 0;
                }

                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }

                return 1;
            }

            result = await userManager.CreateAsync(user, adminConfiguration.Password);
            if (result.Succeeded)
            {
                var roleAssignResult = await userManager.AddToRoleAsync(user, adminConfiguration.Role);

                if (roleAssignResult.Succeeded)
                {
                    return 0;
                }

                var deletionResult = await userManager.DeleteAsync(user);

                if (!deletionResult.Succeeded)
                {
                    Console.WriteLine($"User {user.Id} was created without role.");
                }

                foreach (var error in roleAssignResult.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }

            return 1;
        }
        catch (Exception ex)
        {
            await userManager.RemoveFromRoleAsync(user, user.Role);
            await userManager.DeleteAsync(user);

            Console.WriteLine(ex.Message);

            return 1;
        }
    }
}