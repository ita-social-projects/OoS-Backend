using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class ProviderAdminDto : BaseUserDto
{
    public bool IsDeputy { get; set; }

    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }
}