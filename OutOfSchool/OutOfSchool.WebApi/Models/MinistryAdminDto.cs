using System.ComponentModel.DataAnnotations;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models;

public class MinistryAdminDto : BaseUserDto
{
    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }

    public Guid InstitutionId { get; set; }

    public string InstitutionTitle { get; set; }
}