using System.Collections.Generic;

namespace OutOfSchool.Common.PermissionsModule
{
    public static class PermissionsSeeder
    {
        // basic seed for PermissionsToRole table in DB due to current state of application
        private static readonly IEnumerable<Permissions> SeedAdminPermissions = new List<Permissions>
        {
            Permissions.SystemManagement,
            Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
            Permissions.ApplicationReadManager, Permissions.ApplicationReadParent, Permissions.ApplicationEdit, Permissions.ApplicationRemove, Permissions.ApplicationAddNew,
            Permissions.FavoriteRead, Permissions.FavoriteAddNew, Permissions.FavoriteEdit, Permissions.FavoriteRemove,
            Permissions.ParentRead, Permissions.ParentEdit, Permissions.ParentRemove,
            Permissions.ProviderAddNew, Permissions.ProviderEdit, Permissions.ProviderRead, Permissions.ProviderRemove,
            Permissions.RatingAddNew, Permissions.RatingEdit, Permissions.RatingRead, Permissions.RatingRemove,
            Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove,
            Permissions.UserRead, Permissions.UserEdit,
            Permissions.WorkshopEdit, Permissions.WorkshopRemove, Permissions.WorkshopAddNew,
        };

        private static readonly IEnumerable<Permissions> SeedProviderPermissions = new List<Permissions>
        {
            Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
            Permissions.ApplicationReadManager, Permissions.ApplicationEdit,
            Permissions.ProviderAddNew, Permissions.ProviderEdit, Permissions.ProviderRead, Permissions.ProviderRemove,
            Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove,
            Permissions.UserRead, Permissions.UserEdit,
            Permissions.WorkshopEdit, Permissions.WorkshopRemove, Permissions.WorkshopAddNew,
        };

        private static readonly IEnumerable<Permissions> SeedParentPermissions = new List<Permissions>
        {
            Permissions.AddressAddNew,
            Permissions.ApplicationReadParent, Permissions.ApplicationEdit, Permissions.ApplicationAddNew,
            Permissions.ChildRead, Permissions.ChildAddNew, Permissions.ChildEdit, Permissions.ChildRemove,
            Permissions.FavoriteRead, Permissions.FavoriteAddNew, Permissions.FavoriteEdit, Permissions.FavoriteRemove,
            Permissions.ParentRead, Permissions.ParentEdit, Permissions.ParentRemove,
            Permissions.RatingAddNew, Permissions.RatingEdit, Permissions.RatingRead,
            Permissions.UserRead, Permissions.UserEdit,
        };

        public static string SeedPermissions(string role)
        {
            switch (role)
            {
                case "admin":
                    return SeedAdminPermissions.PackPermissionsIntoString();

                case "provider":
                    return SeedProviderPermissions.PackPermissionsIntoString();

                case "parent":
                    return SeedParentPermissions.PackPermissionsIntoString();
                default:
                    break;
            }
            return null;
        }
    }
}