using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Providers;

public class ProviderSectionItemInfoDto
{
    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }
}

public static class ProviderSectionItemInfoDtoExtensions
{
    public static ProviderSectionItemInfoDto ToInfoDto(this ProviderSectionItem providerSection)
        => new()
        {
            SectionName = providerSection.Name,
            Description = providerSection.Description
        };

    public static List<ProviderSectionItemInfoDto> ToInfoDto(this IEnumerable<ProviderSectionItem> list)
        => list.MapToList(ToInfoDto);
}
