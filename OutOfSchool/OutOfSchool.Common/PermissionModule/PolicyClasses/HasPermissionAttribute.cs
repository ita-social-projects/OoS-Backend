using System;
using Microsoft.AspNetCore.Authorization;

namespace OutOfSchool.Common.PermissionsModule
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(Permissions permission)
            : base(permission.ToString())
        {
        }
    }
}