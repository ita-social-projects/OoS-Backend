using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models;

public class RegionAdminDto : BaseUserDto
{
    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }

    public Guid InstitutionId { get; set; }

    public string InstitutionTitle { get; set; }

    public long CATOTTGId { get; set; }

    public string CATOTTGCategory { get; set; }

    public string CATOTTGName { get; set; }
}

public static class RegionAdminDtoExtensions
{
    public static RegionAdminDto ToDto(this RegionAdmin model)
        => new()
        {
            Id = model.User?.Id,
            Email = model.User?.Email,
            PhoneNumber = model.User?.PhoneNumber,
            LastName = model.User?.LastName,
            MiddleName = model.User?.MiddleName ?? string.Empty,
            FirstName = model.User?.FirstName,
            AccountStatus = model.User?.Convert() ?? default,
            InstitutionId = model.InstitutionId,
            InstitutionTitle = model.Institution?.Title,
            CATOTTGId = model.CATOTTGId,
            CATOTTGCategory = model.CATOTTG?.Category,
            CATOTTGName = model.CATOTTG?.Name,
        };

    public static List<RegionAdminDto> ToDto(this IEnumerable<RegionAdmin> list)
        => list.MapToList(ToDto);

    public static RegionAdminDto ToDto(this RegionAdminBaseDto model)
        => new()
        {
            Id = model.UserId,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            LastName = model.LastName,
            MiddleName = model.MiddleName ?? string.Empty,
            FirstName = model.FirstName,
            InstitutionId = model.InstitutionId,
            CATOTTGId = model.CATOTTGId,
        };

    public static List<RegionAdminDto> ToDto(this IEnumerable<RegionAdminBaseDto> list)
        => list.MapToList(ToDto);
}