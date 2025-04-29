using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class EmailInfoDto
{
    [StringLength(Constants.MaxEmailTypeLength, ErrorMessage = "Email type cannot exceed 60 characters")]
    public string Type { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    [StringLength(Constants.MaxEmailAddressLength)]
    public string Address { get; set; } = null!;
}

public static class EmailDtoExtensions
{
    public static EmailInfoDto ToInfoDto(this Email email)
        => new()
        {
            Type = email.Type,
            Address = email.Address
        };

    public static List<EmailInfoDto> ToInfoDto(this IEnumerable<Email> list)
        => list.MapToList(ToInfoDto);
}