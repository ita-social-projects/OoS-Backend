namespace OutOfSchool.AuthCommon.Util;

public static class IdentityRolesCreator
{
    public static void Create(RoleManager<IdentityRole> manager)
    {
        var roles = new IdentityRole[]
        {
            new IdentityRole {Name = "parent"},
            new IdentityRole {Name = "provider"},
            new IdentityRole {Name = "techadmin"},
            new IdentityRole {Name = "ministryadmin"},
            new IdentityRole {Name = "regionadmin"},
        };
        var newRoles = roles.ExceptBy(manager.Roles.Select(r => r.Name), role => role.Name);

        foreach (var role in newRoles)
        {
            manager.CreateAsync(role).Wait();
        }
    }
}