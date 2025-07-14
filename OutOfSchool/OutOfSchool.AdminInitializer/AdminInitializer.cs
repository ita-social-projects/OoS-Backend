using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OutOfSchool.AdminInitializer.Config;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
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
            UserName = adminConfiguration.Rnokpp,
            FirstName = adminConfiguration.FirstName,
            LastName = adminConfiguration.LastName,
            MiddleName = adminConfiguration.MiddleName,
            Email = adminConfiguration.Email,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = adminConfiguration.Role,
            IsRegistered = true,
            IsBlocked = false,
            EmailConfirmed = true,
        };

        var individual = new Individual
        {
            FirstName = adminConfiguration.FirstName,
            LastName = adminConfiguration.LastName,
            MiddleName = adminConfiguration.MiddleName,
            Rnokpp = adminConfiguration.Rnokpp,
            ExternalRegistryId = Guid.NewGuid(), // TODO: This should be retrieved from aikom
            Gender = Gender.Male, // Default value
            IsRegistered = true,
        };

        List<string> supportedRoles = ["techadmin", "moderator"];
        if (!supportedRoles.Contains(adminConfiguration.Role.ToLower()))
        {
            throw new ArgumentException($"Unsupported role: {adminConfiguration.Role}. Supported roles: techadmin, moderator");
        }

        while (!await dbContext.Database.CanConnectAsync())
        {
            if (retry == maxRetryCount)
            {
                return 1;
            }

            Console.WriteLine($"Can't connect to database. Attempt {retry + 1}");
            await Task.Delay(checkConnectivityDelay);
            retry++;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var existingUser = await userManager.FindByNameAsync(adminConfiguration.Rnokpp);
            if (existingUser != null && !adminConfiguration.Reset)
            {
                Console.WriteLine($"User with username {adminConfiguration.Rnokpp} already exists and reset is not enabled.");
                await transaction.CommitAsync();
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
                existingUser.Role = user.Role;
                existingUser.PasswordHash = userManager.PasswordHasher.HashPassword(existingUser, adminConfiguration.Password);
                
                result = await userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    var existingRoles = await userManager.GetRolesAsync(existingUser);
                    if (existingRoles.Any())
                    {
                        await userManager.RemoveFromRolesAsync(existingUser, existingRoles);
                    }
                    
                    var roleAssignResult = await userManager.AddToRoleAsync(existingUser, adminConfiguration.Role);
                    if (!roleAssignResult.Succeeded)
                    {
                        foreach (var error in roleAssignResult.Errors)
                        {
                            Console.WriteLine($"Failed to assign role: {error.Description}");
                        }
                        return 1;
                    }

                    var existingIndividual = await dbContext.Set<Individual>()
                        .FirstOrDefaultAsync(i => i.Rnokpp == adminConfiguration.Rnokpp);
                    
                    if (existingIndividual != null)
                    {
                        existingIndividual.FirstName = individual.FirstName;
                        existingIndividual.LastName = individual.LastName;
                        existingIndividual.MiddleName = individual.MiddleName;
                        existingIndividual.Rnokpp = individual.Rnokpp;
                        existingIndividual.ExternalRegistryId = individual.ExternalRegistryId;
                        existingIndividual.Gender = individual.Gender;

                        var existingTechAdmin = await dbContext.TechAdmins
                            .FirstOrDefaultAsync(ta => ta.Id == existingIndividual.Id);
                        var existingModerator = await dbContext.Moderators
                            .FirstOrDefaultAsync(m => m.Id == existingIndividual.Id);

                        bool hasSameRole = (adminConfiguration.Role.ToLower() == "techadmin" && existingTechAdmin != null) ||
                                          (adminConfiguration.Role.ToLower() == "moderator" && existingModerator != null);
                        
                        if (hasSameRole)
                        {
                            Console.WriteLine($"User already has {adminConfiguration.Role} role. Keeping existing record unchanged.");
                        }
                        else
                        {
                            if (existingTechAdmin != null)
                            {
                                dbContext.TechAdmins.Remove(existingTechAdmin);
                            }
                            if (existingModerator != null)
                            {
                                dbContext.Moderators.Remove(existingModerator);
                            }

                            switch (adminConfiguration.Role.ToLower())
                            {
                                case "techadmin":
                                {
                                    var newTechAdmin = new TechAdmin
                                    {
                                        Id = existingIndividual.Id
                                    };
                                    dbContext.TechAdmins.Add(newTechAdmin);
                                    break;
                                }
                                case "moderator":
                                {
                                    var newModerator = new Moderator
                                    {
                                        Id = existingIndividual.Id
                                    };
                                    dbContext.Moderators.Add(newModerator);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        individual.UserId = existingUser.Id;
                        dbContext.Set<Individual>().Add(individual);

                        switch (adminConfiguration.Role.ToLower())
                        {
                            case "techadmin":
                            {
                                var newTechAdmin = new TechAdmin
                                {
                                    Id = individual.Id
                                };
                                dbContext.TechAdmins.Add(newTechAdmin);
                                break;
                            }
                            case "moderator":
                            {
                                var newModerator = new Moderator
                                {
                                    Id = individual.Id
                                };
                                dbContext.Moderators.Add(newModerator);
                                break;
                            }
                        }
                    }
                    
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine($"User {existingUser.UserName} updated successfully with role {adminConfiguration.Role}.");
                    return 0;
                }

                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }

                await transaction.RollbackAsync();
                return 1;
            }

            result = await userManager.CreateAsync(user, adminConfiguration.Password);
            if (result.Succeeded)
            {
                var roleAssignResult = await userManager.AddToRoleAsync(user, adminConfiguration.Role);

                if (roleAssignResult.Succeeded)
                {
                    individual.UserId = user.Id;
                    dbContext.Set<Individual>().Add(individual);
                    await dbContext.SaveChangesAsync();
                    
                    switch (adminConfiguration.Role.ToLower())
                    {
                        case "techadmin":
                        {
                            var newTechAdmin = new TechAdmin
                            {
                                Id = individual.Id
                            };
                            dbContext.TechAdmins.Add(newTechAdmin);
                            break;
                        }
                        case "moderator":
                        {
                            var newModerator = new Moderator
                            {
                                Id = individual.Id
                            };
                            dbContext.Moderators.Add(newModerator);
                            break;
                        }
                    }
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    Console.WriteLine($"User {user.UserName} created successfully with role {adminConfiguration.Role}.");
                    return 0;
                }

                var deletionResult = await userManager.DeleteAsync(user);

                if (!deletionResult.Succeeded)
                {
                    Console.WriteLine($"User {user.Id} was created without role and could not be deleted.");
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

            await transaction.RollbackAsync();
            return 1;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error during admin initialization: {ex.Message}");
            return 1;
        }
    }
}