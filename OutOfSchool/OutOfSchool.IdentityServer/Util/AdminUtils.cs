using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using Serilog;

namespace OutOfSchool.IdentityServer.Util
{
    internal static class AdminUtils
    {
        private static readonly Serilog.ILogger LoggerInstance = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        internal static void AddSuperAdmin(UserManager<User> userManager, string environment)
        {
            _ = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _ = environment ?? throw new ArgumentNullException(nameof(environment));

            var superAdminExists =
                userManager.Users.SingleOrDefault(x =>
                    string.Equals(x.Role.ToLower(), Role.Admin.ToString().ToLower())) != null;

            if (!superAdminExists)
            {
                var superAdmin = CreateRequiredSuperAdmin(userManager);
                if (environment.Equals(Environments.Production, StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: send email with essential information by the new admins' template.
                }
            }
            else
            {
                LoggerInstance.Information("Super admin was not created because it already exists in the system");
            }
        }

        private static User CreateRequiredSuperAdmin(UserManager<User> userManager)
        {
            LoggerInstance.Debug("Started creating a new super admin");
            var email = Environment.GetEnvironmentVariable(EnvironmentVariablesNames.SuperAdminEmail)
                        ?? throw new InvalidOperationException("Super admin email cannot be null value");
            var password = Environment.GetEnvironmentVariable(EnvironmentVariablesNames.SuperAdminPassword)
                           ?? throw new InvalidOperationException("Super admin password cannot be null value");

            var superAdminModel = new User
            {
                UserName = email,
                FirstName = "Адмін",
                LastName = "Адмін",
                MiddleName = "Адмін",
                Email = email,
                PhoneNumber = string.Empty,
                CreatingTime = DateTimeOffset.UtcNow,
                Role = Role.Admin.ToString().ToLowerInvariant(),
                IsRegistered = true,
                EmailConfirmed = false,
                IsBlocked = false,
            };

            var identityResult = userManager.CreateAsync(superAdminModel, password).Result;
            var superAdmin = identityResult.Succeeded ?
                superAdminModel :
                throw new InvalidOperationException("Unable to create Super admin, check input data");

            userManager.AddToRoleAsync(superAdmin, superAdmin.Role);
            LoggerInstance.Information("Super admin was successfully created on email {Email}", email);
            return superAdmin;
        }
    }
}