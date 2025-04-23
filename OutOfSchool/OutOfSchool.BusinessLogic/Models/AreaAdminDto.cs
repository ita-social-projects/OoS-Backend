using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models;

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

public static class AreaAdminDtoExtensions
{
    public static AreaAdminDto ToDto(this AreaAdmin areaAdmin)
        => new()
        {
            Id = areaAdmin.User.Id,
            FirstName = areaAdmin.User.FirstName,
            LastName = areaAdmin.User.LastName,
            MiddleName = areaAdmin.User.MiddleName ?? string.Empty,
            PhoneNumber = areaAdmin.User.PhoneNumber,
            Email = areaAdmin.User.Email,

            AccountStatus = areaAdmin.User.Convert(),
            InstitutionId = areaAdmin.InstitutionId, 
            InstitutionTitle = areaAdmin.Institution.Title,
            CATOTTGId = areaAdmin.CATOTTGId, 
            CATOTTGCategory = areaAdmin.CATOTTG.Category,
            CATOTTGName = areaAdmin.CATOTTG.Name,
            RegionId = areaAdmin.CATOTTG.Parent.Parent.Id,
            RegionName = areaAdmin.CATOTTG.Parent.Parent.Name ?? string.Empty,
        };

    public static AreaAdminDto ToDto(this AreaAdminBaseDto areaAdmin)
        => new()
        {
            Id = areaAdmin.UserId,
            FirstName = areaAdmin.FirstName,
            LastName = areaAdmin.LastName,
            MiddleName = areaAdmin.MiddleName,
            PhoneNumber = areaAdmin.PhoneNumber,
            Email = areaAdmin.Email,

            InstitutionId = areaAdmin.InstitutionId,
            CATOTTGId = areaAdmin.CATOTTGId,
        };

    public static List<AreaAdminDto> ToDto(this IEnumerable<AreaAdmin> list)
        => list.MapToList(ToDto);
}