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
    /// <summary>
    /// Converts a <see cref="BaseUserDto"/> instance to a <see cref="TechnicalStaffDto"/> by copying user properties.
    /// </summary>
    /// <param name="dto">The base user DTO to convert.</param>
    /// <returns>A <see cref="TechnicalStaffDto"/> with properties copied from the input. If <c>MiddleName</c> is null, it is set to an empty string.</returns>
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

    /// <summary>
    /// Converts a collection of <see cref="BaseUserDto"/> objects to a list of <see cref="TechnicalStaffDto"/> instances.
    /// </summary>
    /// <param name="list">The collection of base user DTOs to convert.</param>
    /// <returns>A list of <see cref="TechnicalStaffDto"/> objects with properties mapped from the input collection.</returns>
    public static List<TechnicalStaffDto> ToTechnicalStaffDto(this IEnumerable<BaseUserDto> list)
    => list.MapToList(ToTechnicalStaffDto);
}
