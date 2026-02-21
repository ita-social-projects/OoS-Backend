using System;
using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.Common.PermissionsModule;

public static class PermissionPackers
{
    public static string PackPermissionsIntoString(this IEnumerable<Permissions> permissions)
    {
        var bytes = permissions.Select(p => (byte)p).ToArray();

        return Convert.ToBase64String(bytes);
    }

    public static IEnumerable<Permissions> UnpackPermissionsFromString(this string packedPermissions)
    {
        if (string.IsNullOrEmpty(packedPermissions))
        {
            throw new ArgumentNullException(nameof(packedPermissions));
        }

        var bytes = Convert.FromBase64String(packedPermissions);

        return bytes.Select(b => (Permissions)b);
    }

    public static Permissions? FindPermissionViaName(this string permissionName)
    {
        return Enum.TryParse(permissionName, out Permissions permission)
            ? (Permissions?)permission
            : null;
    }
}