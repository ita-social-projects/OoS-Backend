using System.Collections.Generic;

namespace OutOfSchool.Common.PermissionsModule;

public static class PermissionsSeeder
{
    // basic seed for PermissionsToRole table in DB due to current state of application
    private static readonly IEnumerable<Permissions> SeedAdminPermissions = new List<Permissions>
    {
        Permissions.SystemManagement, Permissions.ImpersonalDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead, Permissions.ApplicationEdit, Permissions.ApplicationRemove, Permissions.ApplicationAddNew,
        Permissions.FavoriteRead, Permissions.FavoriteAddNew, Permissions.FavoriteEdit, Permissions.FavoriteRemove,
        Permissions.ParentRead, Permissions.ParentEdit, Permissions.ParentRemove,
        Permissions.ProviderAddNew, Permissions.ProviderEdit, Permissions.ProviderRead, Permissions.ProviderRemove, Permissions.ProviderApprove,
        Permissions.RatingAddNew, Permissions.RatingEdit, Permissions.RatingRead, Permissions.RatingRemove,
        Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove, Permissions.TeacherRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.WorkshopEdit, Permissions.WorkshopRemove, Permissions.WorkshopAddNew,
        Permissions.MinistryAdmins, Permissions.MinistryAdminAddNew, Permissions.MinistryAdminRemove, Permissions.MinistryAdminEdit, Permissions.MinistryAdminRead,
        Permissions.PersonalInfo,
    };

    private static readonly IEnumerable<Permissions> SeedProviderPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead, Permissions.ApplicationEdit,
        Permissions.ProviderAddNew, Permissions.ProviderEdit, Permissions.ProviderRead, Permissions.ProviderRemove, Permissions.ProviderAdmins,
        Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove, Permissions.TeacherRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.WorkshopEdit, Permissions.WorkshopRemove, Permissions.WorkshopAddNew,
        Permissions.PersonalInfo,
    };

    private static readonly IEnumerable<Permissions> SeedProviderAdminPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead, Permissions.ApplicationEdit,
        Permissions.ProviderRead,
        Permissions.ProviderAdmins,
        Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove, Permissions.TeacherRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.WorkshopEdit, Permissions.WorkshopAddNew,
        Permissions.PersonalInfo,
    };

    private static readonly IEnumerable<Permissions> SeedMinistryAdminPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead,
        Permissions.ProviderRead, Permissions.ProviderRemove, Permissions.ProviderApprove,
        Permissions.ParentRead,
        Permissions.ChildRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.TeacherRead,
        Permissions.PersonalInfo,
        Permissions.MinistryAdminRead,
    };

    private static readonly IEnumerable<Permissions> SeedParentPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead,
        Permissions.AddressAddNew,
        Permissions.ApplicationRead, Permissions.ApplicationEdit, Permissions.ApplicationAddNew,
        Permissions.ChildRead, Permissions.ChildAddNew, Permissions.ChildEdit, Permissions.ChildRemove,
        Permissions.FavoriteRead, Permissions.FavoriteAddNew, Permissions.FavoriteEdit, Permissions.FavoriteRemove,
        Permissions.ParentRead, Permissions.ParentEdit, Permissions.ParentRemove,
        Permissions.RatingAddNew, Permissions.RatingEdit, Permissions.RatingRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.PersonalInfo,
    };

    public static string SeedPermissions(string role)
    {
        switch (role.ToLower())
        {
            case "techadmin":
                return SeedAdminPermissions.PackPermissionsIntoString();

            case "provider":
                return SeedProviderPermissions.PackPermissionsIntoString();

            case "provideradmin":
                return SeedProviderAdminPermissions.PackPermissionsIntoString();

            case "ministryadmin":
                return SeedMinistryAdminPermissions.PackPermissionsIntoString();

            case "parent":
                return SeedParentPermissions.PackPermissionsIntoString();

            default:
                break;
        }

        return null;
    }
}