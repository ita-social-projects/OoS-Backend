using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models;

public class ProviderAdminDto : BaseUserDto
{
    public bool IsDeputy { get; set; }

    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }
}