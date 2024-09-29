using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class EmployeeDto : BaseUserDto
{
    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }
}