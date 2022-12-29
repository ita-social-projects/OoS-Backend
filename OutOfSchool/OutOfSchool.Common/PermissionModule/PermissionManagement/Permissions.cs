using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.PermissionsModule;

public enum Permissions : short
{
    NotSet = 0, // error condition

    #region Address Control permissions #1
    [Display(GroupName = "Address", Name = "Read", Description = "Can read Address")]
    AddressRead = 1,
    [Display(GroupName = "Address", Name = "Edit", Description = "Can edit Address item")]
    AddressEdit = 2,
    [Display(GroupName = "Address", Name = "Add new", Description = "Can add a new Address item")]
    AddressAddNew = 3,
    [Display(GroupName = "Address", Name = "Remove", Description = "Can remove Address data")]
    AddressRemove = 4,
    #endregion

    #region Application Control permissions #2
    [Display(GroupName = "Application", Name = "Read", Description = "Can read Application")]
    ApplicationRead = 10,
    [Display(GroupName = "Application", Name = "Edit", Description = "Can edit Application item")]
    ApplicationEdit = 11,
    [Display(GroupName = "Application", Name = "Add new", Description = "Can add a new Application item")]
    ApplicationAddNew = 12,
    [Display(GroupName = "Application", Name = "Remove", Description = "Can remove Application data")]
    ApplicationRemove = 13,
    #endregion

    #region Children Control permissions #3
    [Display(GroupName = "Child", Name = "Read", Description = "Can read Child")]
    ChildRead = 20,
    [Display(GroupName = "Child", Name = "Edit", Description = "Can edit Child item")]
    ChildEdit = 21,
    [Display(GroupName = "Child", Name = "Add new", Description = "Can add a new Child item")]
    ChildAddNew = 22,
    [Display(GroupName = "Child", Name = "Remove", Description = "Can remove Child data")]
    ChildRemove = 23,
    #endregion

    #region Favorite control permissions #4
    [Display(GroupName = "Favorite", Name = "Read", Description = "Can read Favorite")]
    FavoriteRead = 30,
    [Display(GroupName = "Favorite", Name = "Edit", Description = "Can edit Favorite item")]
    FavoriteEdit = 31,
    [Display(GroupName = "Favorite", Name = "Add new", Description = "Can add a new Favorite item")]
    FavoriteAddNew = 32,
    [Display(GroupName = "Favorite", Name = "Remove", Description = "Can remove Favorite data")]
    FavoriteRemove = 33,
    #endregion

    #region Parent Control permissions #5
    [Display(GroupName = "Parent", Name = "Read", Description = "Can read Parent")]
    ParentRead = 40,
    [Display(GroupName = "Parent", Name = "Edit", Description = "Can edit Parent item")]
    ParentEdit = 41,
    [Display(GroupName = "Parent", Name = "Add new", Description = "Can add a new Parent item")]
    ParentAddNew = 42,
    [Display(GroupName = "Parent", Name = "Remove", Description = "Can remove Parent data")]
    ParentRemove = 43,
    #endregion

    #region Provider Control permissions #6
    [Display(GroupName = "Provider", Name = "Read", Description = "Can read Provider")]
    ProviderRead = 50,
    [Display(GroupName = "Provider", Name = "Edit", Description = "Can edit Provider item")]
    ProviderEdit = 51,
    [Display(GroupName = "Provider", Name = "Add new", Description = "Can add a new Provider item")]
    ProviderAddNew = 52,
    [Display(GroupName = "Provider", Name = "Remove", Description = "Can remove Provider data")]
    ProviderRemove = 53,
    [Display(GroupName = "Provider", Name = "Provider Admins", Description = "Can create and manage provider admins")]
    ProviderAdmins = 54,
    [Display(GroupName = "Provider", Name = "Provider Approve", Description = "Can approve Provider and License")]
    ProviderApprove = 55,
    #endregion

    #region Rating control permissions #7
    [Display(GroupName = "Rating", Name = "Read", Description = "Can read Rating")]
    RatingRead = 60,
    [Display(GroupName = "Rating", Name = "Edit", Description = "Can edit Rating item")]
    RatingEdit = 61,
    [Display(GroupName = "Rating", Name = "Add new", Description = "Can add a new Rating item")]
    RatingAddNew = 62,
    [Display(GroupName = "Rating", Name = "Remove", Description = "Can remove Rating data")]
    RatingRemove = 63,
    #endregion

    #region Teacher control permissions #8
    [Display(GroupName = "Teacher", Name = "Read", Description = "Can read Teacher")]
    TeacherRead = 70,
    [Display(GroupName = "Teacher", Name = "Edit", Description = "Can edit Teacher item")]
    TeacherEdit = 71,
    [Display(GroupName = "Teacher", Name = "Add new", Description = "Can add a new Teacher item")]
    TeacherAddNew = 72,
    [Display(GroupName = "Teacher", Name = "Remove", Description = "Can remove Teacher data")]
    TeacherRemove = 73,
    #endregion

    #region User control permissions #9
    [Display(GroupName = "User", Name = "Read", Description = "Can read User")]
    UserRead = 80,
    [Display(GroupName = "User", Name = "Edit", Description = "Can edit User item")]
    UserEdit = 81,
    [Display(GroupName = "User", Name = "Add new", Description = "Can add a new User item")]
    UserAddNew = 82,
    [Display(GroupName = "User", Name = "Remove", Description = "Can remove User data")]
    UserRemove = 83,
    [Display(GroupName = "User", Name = "Personal Info", Description = "Can read or modify your own personal information")]
    PersonalInfo = 84,
    #endregion

    #region Workshop Control permissions #10
    [Display(GroupName = "Workshop", Name = "Read", Description = "Can read Workshop")]
    WorkshopRead = 90,
    [Display(GroupName = "Workshop", Name = "Edit", Description = "Can edit a Workshop item")]
    WorkshopEdit = 91,
    [Display(GroupName = "Workshop", Name = "Add new", Description = "Can add a new Workshop item")]
    WorkshopAddNew = 92,
    [Display(GroupName = "Workshop", Name = "Remove", Description = "Can remove Workshop data")]
    WorkshopRemove = 93,
    #endregion

    #region System Management and admin permissions #11
    [Display(GroupName = "SystemManaging", Name = "SystemManagement", Description = "Permissions to manage system conditions and specific data")]
    SystemManagement = 100,
    [Display(GroupName = "SystemManaging", Name = "ReadImpersonalData", Description = "For non-admin users to get specific data as city, class, status etc")]
    ImpersonalDataRead = 101,
    [Display(GroupName = "SystemManaging", Name = "ReadLoglData", Description = "Can read log data")]
    LogDataRead = 102,
    [Display(GroupName = "SystemManaging", Name = "SuperAdmin", Description = "access to all actions covered with [HasPermission] attribute")]
    AccessAll = short.MaxValue,
    #endregion

    #region MinistryAdmin Control permissions #12
    [Display(GroupName = "MinistryAdmin", Name = "Read", Description = "Can read ministry admin")]
    MinistryAdminRead = 110,
    [Display(GroupName = "MinistryAdmin", Name = "Edit", Description = "Can edit ministry admin")]
    MinistryAdminEdit = 111,
    [Display(GroupName = "MinistryAdmin", Name = "Add new", Description = "Can add a new ministry admin")]
    MinistryAdminAddNew = 112,
    [Display(GroupName = "MinistryAdmin", Name = "Remove", Description = "Can remove ministry admin")]
    MinistryAdminRemove = 113,
    [Display(GroupName = "MinistryAdmin", Name = "Ministry Admins", Description = "Can manage ministry admins")]
    MinistryAdmins = 114,
    #endregion
}