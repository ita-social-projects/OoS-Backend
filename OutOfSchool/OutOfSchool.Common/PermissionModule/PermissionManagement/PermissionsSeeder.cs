using System.Collections.Generic;

namespace OutOfSchool.Common.PermissionsModule
{
    // permissions for role seeding class.
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

        // method which returns set of packed permissions for specific role
        public static string SeedPermissions(string role)
        {
            string result = string.Empty;
            switch (role)
            {
                case "admin":
                    result = SeedAdminPermissions.PackPermissionsIntoString();
                    break;

                case "provider":
                    result = SeedProviderPermissions.PackPermissionsIntoString();
                    break;

                case "parent":
                    result = SeedParentPermissions.PackPermissionsIntoString();
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}