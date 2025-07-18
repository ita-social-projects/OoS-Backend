using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Util.CustomComparers;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopDescriptionItemDto
{
    [Required]
    [MaxLength(200)]
    public string SectionName { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid WorkshopId { get; set; }
}

public static class WorkshopDescriptionItemDtoExtensions
{
    public static WorkshopDescriptionItemDraft ToDraft(this WorkshopDescriptionItemDto dto)
        => new()
        {
            SectionName = dto.SectionName,
            Description = dto.Description,
        };

    public static List<WorkshopDescriptionItemDraft> ToDraft(this IEnumerable<WorkshopDescriptionItemDto> list)
        => list.MapToList(ToDraft);

    public static WorkshopDescriptionItem ToModel(this WorkshopDescriptionItemDto dto)
        => new()
        {
            SectionName = dto.SectionName,
            Description = dto.Description,
            WorkshopId = dto.WorkshopId,
        };

    public static List<WorkshopDescriptionItem> ToModel(this IEnumerable<WorkshopDescriptionItemDto> list)
        => list.MapToList(ToModel);

    public static WorkshopDescriptionItem SetToModel(this WorkshopDescriptionItemDto dto, WorkshopDescriptionItem model)
    {
        model.SectionName = dto.SectionName;
        model.Description = dto.Description;
        model.WorkshopId = dto.WorkshopId;

        return model;
    }

    public static List<WorkshopDescriptionItem> SetToModel(this IEnumerable<WorkshopDescriptionItemDto> dtoList, IEnumerable<WorkshopDescriptionItem> modelList)
    {
        var result = modelList.Where(dtr => dtr.IsDeleted == false).ToList();

        for (int i = 0; i < result.Count; i++)
        {
            if (!dtoList.ToModel().Contains(result[i], new WorkshopDescriptionItemComparerWithoutKeys()))
            {
                result.RemoveAt(i);
            }
        }

        foreach (var dto in dtoList)
        {
            if (!result.Contains(dto.ToModel(), new WorkshopDescriptionItemComparerWithoutKeys()))
            {
                var newModelItem = new WorkshopDescriptionItem();
                result.Add(dto.SetToModel(newModelItem));
            }
        }

        return result;
    }

    public static WorkshopDescriptionItemDto ToDto(this WorkshopDescriptionItemDraft model)
        => new()
        {
            SectionName = model.SectionName,
            Description = model.Description,
        };

    public static List<WorkshopDescriptionItemDto> ToDto(this IEnumerable<WorkshopDescriptionItemDraft> list)
        => list.MapToList(ToDto);

    public static WorkshopDescriptionItemDto ToDto(this WorkshopDescriptionItem model)
        => new()
        {
            SectionName = model.SectionName,
            Description = model.Description,
            WorkshopId = model.WorkshopId,
        };

    public static List<WorkshopDescriptionItemDto> ToDto(this IEnumerable<WorkshopDescriptionItem> list)
        => list.MapToList(ToDto);

    public static List<WorkshopDescriptionItemDto> ToNotDeletedDto(this IEnumerable<WorkshopDescriptionItem> list)
        => list.MapNonDeletedToList(ToDto);
}