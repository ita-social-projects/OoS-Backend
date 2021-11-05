using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OutOfSchool.Common.PermissionsModule;

namespace OutOfSchool.Common.PermissionsModule
{
    public static class PermissionChecker
    {
        public static bool ThisPermissionIsAllowed(this string packedPermissions, string permissionName)
        {
            var usersPermissions = packedPermissions.UnpackPermissionsFromString().ToArray();

            if (!Enum.TryParse(permissionName, true, out Permissions permissionToCheck))
            {
                throw new InvalidEnumArgumentException($"{permissionName} could not be converted to a {nameof(Permissions)}.");
            }

            return usersPermissions.UserHasThisPermission(permissionToCheck);
        }

        public static bool UserHasThisPermission(this Permissions[] usersPermissions, Permissions permissionToCheck)
        {
            return usersPermissions.Contains(permissionToCheck) || usersPermissions.Contains(Permissions.AccessAll);
        }
    }
}