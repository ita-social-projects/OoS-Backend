using System.Collections.Generic;

namespace OutOfSchool.Common.PermissionsModule;

public static class PermissionsSeeder
{
    // basic seed for PermissionsToRole table in DB due to current state of application
    private static readonly IEnumerable<Permissions> SeedAdminPermissions = new List<Permissions>
    {
        Permissions.SystemManagement, Permissions.ImpersonalDataRead, Permissions.LogDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead, Permissions.ApplicationEdit, Permissions.ApplicationRemove,
        Permissions.ApplicationAddNew,
        Permissions.FavoriteRead, Permissions.FavoriteAddNew, Permissions.FavoriteEdit, Permissions.FavoriteRemove,
        Permissions.ParentRead, Permissions.ParentEdit, Permissions.ParentRemove, Permissions.ParentBlock,
        Permissions.ChildRemove,
        Permissions.ProviderAddNew, Permissions.ProviderEdit, Permissions.ProviderRead, Permissions.ProviderRemove,
        Permissions.ProviderApprove, Permissions.ProviderBlock,
        Permissions.RatingAddNew, Permissions.RatingEdit, Permissions.RatingRead, Permissions.RatingRemove,
        Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove, Permissions.TeacherRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.WorkshopEdit, Permissions.WorkshopRemove, Permissions.WorkshopAddNew, Permissions.WorkshopApprove,
        Permissions.MinistryAdmins, Permissions.MinistryAdminAddNew, Permissions.MinistryAdminRemove,
        Permissions.MinistryAdminEdit, Permissions.MinistryAdminRead,
        Permissions.RegionAdmins, Permissions.RegionAdminAddNew, Permissions.RegionAdminRemove,
        Permissions.RegionAdminEdit, Permissions.RegionAdminRead, Permissions.RegionAdminBlock,
        Permissions.AreaAdmins, Permissions.AreaAdminAddNew, Permissions.AreaAdminRemove,
        Permissions.AreaAdminEdit, Permissions.AreaAdminRead, Permissions.AreaAdminBlock,
        Permissions.PersonalInfo,
        Permissions.AdminDataRead,
        Permissions.CompetitiveEventRead,
    };

    private static readonly IEnumerable<Permissions> SeedProviderPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead, Permissions.ApplicationEdit,
        Permissions.ProviderAddNew, Permissions.ProviderEdit, Permissions.ProviderRead, Permissions.ProviderRemove,
        Permissions.Employees,
        Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove, Permissions.TeacherRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.WorkshopEdit, Permissions.WorkshopRemove, Permissions.WorkshopAddNew,
        Permissions.PersonalInfo,
        Permissions.CompetitiveEventRead, Permissions.CompetitiveEventAddNew, Permissions.CompetitiveEventEdit, Permissions.CompetitiveEventRemove,
        Permissions.PositionRead, Permissions.PositionEdit, Permissions.PositionRemove, Permissions.PositionAddNew,
    };

    private static readonly IEnumerable<Permissions> SeedEmployeePermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead, Permissions.ApplicationEdit,
        Permissions.ProviderRead,
        Permissions.Employees,
        Permissions.TeacherAddNew, Permissions.TeacherEdit, Permissions.TeacherRemove, Permissions.TeacherRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.WorkshopEdit, Permissions.WorkshopAddNew,
        Permissions.PersonalInfo,
        Permissions.CompetitiveEventRead,
    };

    private static readonly IEnumerable<Permissions> SeedMinistryAdminPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead, Permissions.LogDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead,
        Permissions.ProviderRead, Permissions.ProviderRemove, Permissions.ProviderApprove, Permissions.ProviderBlock,
        Permissions.ParentRead, Permissions.ParentBlock,
        Permissions.ChildRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.TeacherRead,
        Permissions.PersonalInfo,
        Permissions.MinistryAdminRead,
        Permissions.WorkshopEdit, Permissions.WorkshopApprove,
        Permissions.RegionAdminAddNew, Permissions.RegionAdminRead, Permissions.RegionAdminEdit,
        Permissions.RegionAdminRemove, Permissions.RegionAdminBlock,
        Permissions.AreaAdminAddNew, Permissions.AreaAdminRead, Permissions.AreaAdminEdit,
        Permissions.AreaAdminRemove, Permissions.AreaAdminBlock,
        Permissions.AdminDataRead,
        Permissions.CompetitiveEventRead,
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
        Permissions.CompetitiveEventRead,
    };

    private static readonly IEnumerable<Permissions> SeedRegionAdminPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead, Permissions.LogDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead,
        Permissions.ProviderRead, Permissions.ProviderRemove, Permissions.ProviderApprove, Permissions.ProviderBlock,
        Permissions.ParentRead,
        Permissions.ChildRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.TeacherRead,
        Permissions.PersonalInfo,
        Permissions.RegionAdminRead, Permissions.RegionAdminEdit,
        Permissions.AreaAdminAddNew, Permissions.AreaAdminRead, Permissions.AreaAdminEdit,
        Permissions.AreaAdminRemove, Permissions.AreaAdminBlock,
        Permissions.WorkshopEdit, Permissions.WorkshopApprove,
        Permissions.AdminDataRead,
        Permissions.CompetitiveEventRead,
    };

    private static readonly IEnumerable<Permissions> SeedAreaAdminPermissions = new List<Permissions>
    {
        Permissions.ImpersonalDataRead, Permissions.LogDataRead,
        Permissions.AddressAddNew, Permissions.AddressEdit, Permissions.AddressRead, Permissions.AddressRemove,
        Permissions.ApplicationRead,
        Permissions.ProviderRead, Permissions.ProviderRemove, Permissions.ProviderApprove, Permissions.ProviderBlock,
        Permissions.ParentRead,
        Permissions.ChildRead,
        Permissions.UserRead, Permissions.UserEdit,
        Permissions.TeacherRead,
        Permissions.PersonalInfo,
        Permissions.AreaAdminRead, Permissions.AreaAdminEdit,
        Permissions.WorkshopEdit, Permissions.WorkshopApprove,
        Permissions.AdminDataRead,
        Permissions.CompetitiveEventRead,
    };

    private static readonly IEnumerable<Permissions> SeedModeratorPermissions = new List<Permissions>
    {
        Permissions.ProviderRead, Permissions.ProviderApprove,
        Permissions.WorkshopRead, Permissions.WorkshopApprove,
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

            case "employee":
                return SeedEmployeePermissions.PackPermissionsIntoString();

            case "ministryadmin":
                return SeedMinistryAdminPermissions.PackPermissionsIntoString();

            case "parent":
                return SeedParentPermissions.PackPermissionsIntoString();

            case "regionadmin":
                return SeedRegionAdminPermissions.PackPermissionsIntoString();

            case "areaadmin":
                return SeedAreaAdminPermissions.PackPermissionsIntoString();

            case "moderator":
                return SeedModeratorPermissions.PackPermissionsIntoString();
            default:
                break;
        }

        return null;
    }
}