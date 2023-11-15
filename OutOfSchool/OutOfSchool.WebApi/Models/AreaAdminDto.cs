using System.ComponentModel.DataAnnotations;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Models;

public class AreaAdminDto : BaseUserDto
{
    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }

    public Guid InstitutionId { get; set; }

    public string InstitutionTitle { get; set; }

    public long CATOTTGId { get; set; }

    public string CATOTTGCategory { get; set; }

    public string CATOTTGName { get; set; }

    public long RegionId { get; set; }

    public string RegionName { get; set; }
}