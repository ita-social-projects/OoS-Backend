using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models;

public class ShortUserDto : BaseUserDto
{
    public string UserName { get; set; }

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
    public static ShortUserDto ToShortUser(this User user)
    => new()
    {
        Id = user.Id,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        LastName = user.LastName,
        MiddleName = user.MiddleName ?? string.Empty,
        FirstName = user.FirstName,
        UserName = user.UserName,
        Role = user.Role,
        IsRegistered = user.IsRegistered,
        EmailConfirmed = user.EmailConfirmed,
    };

    public static ShortUserDto ToShortUser(this OutOfSchool.Services.Models.Parent parent)
    => new()
    {
        Id = parent.User.Id,
        Email = parent.User.Email,
        PhoneNumber = parent.User.PhoneNumber,
        LastName = parent.User.LastName,
        MiddleName = parent.User.MiddleName ?? string.Empty,
        FirstName = parent.User.FirstName,
        UserName = parent.User.UserName,
        Role = parent.User.Role,
        IsRegistered = parent.User.IsRegistered,
        EmailConfirmed = parent.User.EmailConfirmed,
        Gender = parent.Gender,
        DateOfBirth = parent.DateOfBirth,
    };
}