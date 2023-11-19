using System.ComponentModel.DataAnnotations;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models.Admins;

public class BaseAdminDto : BaseUserDto
{
    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }
}