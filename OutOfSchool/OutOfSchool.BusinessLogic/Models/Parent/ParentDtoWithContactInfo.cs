using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists;

namespace OutOfSchool.BusinessLogic.Models;

public class ParentDtoWithContactInfo : ParentDTO
{
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PhoneNumber { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string FirstName { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; }

    public bool IsBlocked { get; set; }
}

public static class ParentDtoWithContactInfoExtensions
{
    public static ParentDtoWithContactInfo ToContactInfoDto(this OutOfSchool.Services.Models.Parent parent)
        => new()
        {
            Id = parent.Id,
            UserId = parent.UserId,
            Gender = parent.Gender ?? Gender.Male,
            Email = parent.User.Email,
            EmailConfirmed = parent.User.EmailConfirmed,
            PhoneNumber = parent.User.PhoneNumber,
            LastName = parent.User.LastName,
            MiddleName = parent.User.MiddleName,
            FirstName = parent.User.FirstName,
            IsBlocked = parent.User.IsBlocked,
        };

    public static List<ParentDtoWithContactInfo> ToContactInfoDto(this IEnumerable<OutOfSchool.Services.Models.Parent> list)
        => list.MapNonDeletedToList(ToContactInfoDto);
    
    public static ParentDtoWithContactInfo ToContactInfoDto(this ParentInfoForChatList parent)
        => new()
        {
            Id = parent.Id,
            UserId = parent.UserId,
            Gender = parent.Gender ?? Gender.Male,
            Email = parent.Email,
            PhoneNumber = parent.PhoneNumber,
            LastName = parent.LastName,
            MiddleName = parent.MiddleName,
            FirstName = parent.FirstName,
        };
}