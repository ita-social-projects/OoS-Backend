using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class ShortUserDto : BaseUserDto
{
    public string Role { get; set; }

    public bool IsRegistered { get; set; }

    public bool EmailConfirmed { get; set; }

    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Gender? Gender { get; set; }

    [DataType(DataType.Date)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [CustomAge(MinAge = Constants.AdultAge, ErrorMessage = Constants.DayOfBirthErrorMessage)]
    public DateTime? DateOfBirth { get; set; }
}

public static class ShortUserDtoExtensions
{
    /// <summary>
    /// Converts a <see cref="User"/> object to a <see cref="ShortUserDto"/> with selected user details.
    /// </summary>
    /// <param name="user">The user entity to convert.</param>
    /// <returns>A <see cref="ShortUserDto"/> containing basic information from the user.</returns>
    public static ShortUserDto ToShortUser(this User user)
    => new()
    {
        Id = user.Id,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        LastName = user.LastName,
        MiddleName = user.MiddleName ?? string.Empty,
        FirstName = user.FirstName,
        Role = user.Role,
        IsRegistered = user.IsRegistered,
        EmailConfirmed = user.EmailConfirmed,
    };

    /// <summary>
    /// Converts a <see cref="Parent"/> object to a <see cref="ShortUserDto"/>, mapping user and parent properties.
    /// </summary>
    /// <param name="parent">The parent object to convert.</param>
    /// <returns>A <see cref="ShortUserDto"/> populated with data from the parent and its associated user.</returns>
    public static ShortUserDto ToShortUser(this OutOfSchool.Services.Models.Parent parent)
    => new()
    {
        Id = parent.User?.Id,
        Email = parent.User?.Email,
        PhoneNumber = parent.User?.PhoneNumber,
        LastName = parent.User?.LastName,
        MiddleName = parent.User?.MiddleName ?? string.Empty,
        FirstName = parent.User?.FirstName,
        Role = parent.User?.Role,
        IsRegistered = parent.User?.IsRegistered ?? default,
        EmailConfirmed = parent.User?.EmailConfirmed ?? default,
        Gender = parent.Gender,
        DateOfBirth = parent.DateOfBirth,
    };
}