// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OutOfSchool.Common.PermissionsModule
{
    public static class PermissionChecker
    {
        /// <summary>
        /// This is used by the policy provider to check the permission name string.
        /// </summary>
        public static bool ThisPermissionIsAllowed(this string packedPermissions, string permissionName)
        {
            var usersPermissions = new string[] { packedPermissions };

            return usersPermissions.UserHasThisPermission(permissionName);
        }

        /// <summary>
        /// This is the main checker of whether a user permissions allows them to access something with the given permission.
        /// </summary>
        public static bool UserHasThisPermission(this string[] usersPermissions, string permissionToCheck)
        {
            return usersPermissions.Contains(permissionToCheck) || usersPermissions.Contains("AccessAll");
        }
    }
}