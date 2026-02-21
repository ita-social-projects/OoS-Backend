using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;

namespace OutOfSchool.BusinessLogic.Models.SocialGroup;

public class SocialGroupDto
{
    public long Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public static class SocialGroupDtoExtensions
{
    public static OutOfSchool.Services.Models.SocialGroup ToModel(this SocialGroupDto socialGroup)
        => new()
        {
            Id = socialGroup.Id,
            Name = socialGroup.Name,
        };

    public static SocialGroupDto ToDto(this OutOfSchool.Services.Models.SocialGroup socialGroup, LocalizationType localization = LocalizationType.Ua)
        => new()
        {
            Id = socialGroup.Id,
            Name = localization == LocalizationType.Ua
                ? socialGroup.Name
                : socialGroup.NameEn,
        };

    public static List<SocialGroupDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.SocialGroup> list, LocalizationType localization = LocalizationType.Ua)
        => list.MapToList(x => ToDto(x, localization));

    public static List<SocialGroupDto> ToNotDeletedDto(this IEnumerable<OutOfSchool.Services.Models.SocialGroup> list, LocalizationType localization = LocalizationType.Ua)
        => list.MapNonDeletedToList(x => ToDto(x, localization));
}