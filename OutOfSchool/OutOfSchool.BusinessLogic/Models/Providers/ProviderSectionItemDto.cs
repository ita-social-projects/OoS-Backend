using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Providers;

public class ProviderSectionItemDto
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid ProviderId { get; set; }
}

public static class ProviderSectionItemDtoExtensions
{
    public static ProviderSectionItem ToModel(this ProviderSectionItemDto dto)
        => new()
        {
            Id = dto.Id,
            Name = dto.SectionName,
            Description = dto.Description,
            ProviderId = dto.ProviderId
        };

    public static List<ProviderSectionItem> ToModel(this IEnumerable<ProviderSectionItemDto> list)
        => list.MapToList(ToModel);

    public static ProviderSectionItemDto ToDto(this ProviderSectionItem model)
        => new()
        {
            Id = model.Id,
            SectionName = model.Name,
            Description = model.Description,
            ProviderId = model.ProviderId
        };

    public static List<ProviderSectionItemDto> ToDto(this IEnumerable<ProviderSectionItem> list)
        => list.MapToList(ToDto);

    public static List<ProviderSectionItemDto> ToNotDeletedDto(this IEnumerable<ProviderSectionItem> list)
        => list.MapNonDeletedToList(ToDto);
}