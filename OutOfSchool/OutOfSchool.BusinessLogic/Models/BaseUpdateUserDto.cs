using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models;
public class BaseUpdateUserDto
{
    public string Id { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string PhoneNumber { get; set; }
}

public static class BaseUpdateUserDtoExtensions
{
    public static MinistryAdminBaseUpdateDto ToMinistryAdminDto(this BaseUpdateUserDto user)
        => new()
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };

    public static User SetToModel(this BaseUpdateUserDto dto, User model)
    {
        if (!String.IsNullOrWhiteSpace(dto.Email))
        {
            model.Email = dto.Email;
        }
        model.PhoneNumber = dto.PhoneNumber;

        return model;
    }

    public static RegionAdminBaseUpdateDto ToRegionAdminDto(this BaseUpdateUserDto user)
        => new()
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };

    public static AreaAdminBaseUpdateDto ToAreaAdminDto(this BaseUpdateUserDto user)
        => new()
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };
}
