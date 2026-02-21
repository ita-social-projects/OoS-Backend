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
    public static ProviderSectionItem SetToModel(this ProviderSectionItemDto dto, ProviderSectionItem model)
    {
        model.Name = dto.SectionName;
        model.Description = dto.Description;

        return model;
    }

    public static List<ProviderSectionItem> SetToModel(this IEnumerable<ProviderSectionItemDto> dtoList, IEnumerable<ProviderSectionItem> modelList)
    {
        var result = modelList.Where(dtr => !dtr.IsDeleted).ToList();
        var dtoIds = dtoList.Where(x => x.Id != Guid.Empty).Select(x => x.Id).ToHashSet();

        result.RemoveAll(x => !dtoIds.Contains(x.Id));
        var modelIds = result.ToDictionary(x => x.Id);

        foreach (var dto in dtoList)
        {
            if (modelIds.TryGetValue(dto.Id, out var existingItem))
            {
                existingItem.Name = dto.SectionName;
                existingItem.Description = dto.Description;
            }
            else
            {
                var newModelItem = new ProviderSectionItem();
                result.Add(dto.SetToModel(newModelItem));
            }
        }

        return result;
    }

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