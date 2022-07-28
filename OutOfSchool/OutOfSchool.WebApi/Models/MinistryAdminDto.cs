using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models;

public class MinistryAdminDto : BaseUserDto
{
    public AccountStatus AccountStatus { get; set; }

    public Guid InstitutionId { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; }

    public string InstitutionTitle { get; set; }
}