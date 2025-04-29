using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models;

public class MinistryAdminDto : BaseUserDto
{
    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }

    public Guid InstitutionId { get; set; }

    public string InstitutionTitle { get; set; }
}

public static class MinistryAdminDtoExtensions
{
    public static MinistryAdminDto ToMinistryAdminDto(this InstitutionAdmin model)
        => new()
        {
            Id = model.UserId,
            Email = model.User?.Email,
            PhoneNumber = model.User?.PhoneNumber,
            LastName = model.User?.LastName,
            MiddleName = model.User?.MiddleName ?? string.Empty,
            FirstName = model.User?.FirstName,
            AccountStatus = model.User?.Convert() ?? default,
            InstitutionId = model.InstitutionId,
            InstitutionTitle = model.Institution.Title,
        };

    public static List<MinistryAdminDto> ToMinistryAdminDto(this IEnumerable<InstitutionAdmin> list)
        => list.MapToList(ToMinistryAdminDto);

    public static MinistryAdminDto ToMinistryAdminDto(this MinistryAdminBaseDto model)
        => new()
        {
            Id = model.UserId,            
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            LastName = model.LastName,
            MiddleName = model.MiddleName ?? string.Empty,
            FirstName = model.FirstName,
            InstitutionId = model.InstitutionId,
        };
}