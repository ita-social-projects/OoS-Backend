using System;
using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.Common.PermissionsModule
{
    public static class PermissionPackers
    {
        public static string PackPermissionsIntoString(this IEnumerable<Permissions> permissions)
        {
            return permissions.Aggregate(string.Empty, (s, permission) => s + (char)permission);
        }

        public static IEnumerable<Permissions> UnpackPermissionsFromString(this string packedPermissions)
        {
            if (packedPermissions == null)
            {
                throw new ArgumentNullException(nameof(packedPermissions));
            }

            foreach (var character in packedPermissions)
            {
                yield return (Permissions)character;
            }
        }

        public static Permissions? FindPermissionViaName(this string permissionName)
        {
            return Enum.TryParse(permissionName, out Permissions permission)
                ? (Permissions?)permission
                : null;
        }
    }
}