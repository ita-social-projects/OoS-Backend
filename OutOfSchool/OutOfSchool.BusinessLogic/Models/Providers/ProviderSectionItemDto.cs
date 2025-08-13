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
        model.Id = dto.Id;
        model.Name = dto.SectionName;
        model.Description = dto.Description;
        model.ProviderId = dto.ProviderId;

        return model;
    }

    public static List<ProviderSectionItem> SetToModel(this IEnumerable<ProviderSectionItemDto> dtoList, IEnumerable<ProviderSectionItem> modelList, Guid providerId)
    {
        var providerSectionItemsDict = modelList.Where(x => !x.IsDeleted).ToDictionary(key => key.Id);
        var result = new List<ProviderSectionItem>();

        foreach (var dtoItem in dtoList)
        {
            var id = dtoItem.Id == Guid.Empty ? Guid.NewGuid() : dtoItem.Id;

            if (providerSectionItemsDict.TryGetValue(id, out var existingItem))
            {
                existingItem.Name = dtoItem.SectionName;
                existingItem.Description = dtoItem.Description;

                result.Add(existingItem);
                providerSectionItemsDict.Remove(id);
            }
            else
            {
                var newEntity = new ProviderSectionItem()
                {
                    Id = id,
                    Description = dtoItem.Description,
                    Name = dtoItem.SectionName,
                    ProviderId = providerId
                };
                result.Add(newEntity);
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