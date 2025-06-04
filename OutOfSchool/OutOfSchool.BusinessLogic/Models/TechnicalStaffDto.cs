using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class TechnicalStaffDto : BaseUserDto
{
    [EnumDataType(typeof(AccountStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public AccountStatus AccountStatus { get; set; }
}

public static class TechnicalStaffDtoExtensions
{
    public static TechnicalStaffDto ToTechnicalStaffDto(this BaseUserDto dto)
        => new()
        {
            Id = dto.Id,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName ?? string.Empty,
            FirstName = dto.FirstName,
        };

    public static List<TechnicalStaffDto> ToTechnicalStaffDto(this IEnumerable<BaseUserDto> list)
        => list.MapToList(ToTechnicalStaffDto);
}
