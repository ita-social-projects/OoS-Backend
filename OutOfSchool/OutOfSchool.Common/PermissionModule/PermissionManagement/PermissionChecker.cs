using System;
using System.ComponentModel;
using System.Linq;

namespace OutOfSchool.Common.PermissionsModule;

public static class PermissionChecker
{
    public static bool ThisPermissionIsAllowed(this string packedPermissions, PermissionRequirement requirement)
    {
        var usersPermissions = packedPermissions.UnpackPermissionsFromString().ToArray();

        if (!Enum.TryParse(requirement.PermissionName, true, out Permissions permissionToCheck))
        {
            throw new InvalidEnumArgumentException($"{requirement.PermissionName} could not be converted to a {nameof(Permissions)}.");
        }

        return usersPermissions.UserHasThisPermission(permissionToCheck);
    }

    public static bool UserHasThisPermission(this Permissions[] usersPermissions, Permissions permissionToCheck)
    {
        return usersPermissions.Contains(permissionToCheck) || usersPermissions.Contains(Permissions.AccessAll);
    }
}