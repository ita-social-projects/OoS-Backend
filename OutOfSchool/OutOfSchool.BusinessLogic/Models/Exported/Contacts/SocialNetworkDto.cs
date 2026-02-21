using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.Exported.Contacts;

public class SocialNetworkInfoDto
{
    [EnumDataType(typeof(SocialNetworkContactType), ErrorMessage = Constants.EnumErrorMessage)]
    public SocialNetworkContactType Type { get; set; }

    [StringLength(Constants.MaxUnifiedUrlLength, ErrorMessage = "URL cannot exceed allowed length.")]
    public string Url { get; set; } = string.Empty;
}

public static class SocialNetworkInfoDtoExtensions
{
    public static SocialNetworkInfoDto ToInfoDto(this SocialNetwork socialNetwork)
        => new()
        {
            Type = socialNetwork.Type,
            Url = socialNetwork.Url
        };

    public static List<SocialNetworkInfoDto> ToInfoDto(this IEnumerable<SocialNetwork> list)
        => list.MapToList(ToInfoDto);
}