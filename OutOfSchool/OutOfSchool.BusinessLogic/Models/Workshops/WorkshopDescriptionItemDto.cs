using System.ComponentModel.DataAnnotations;
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

        return model;
    }

    public static List<WorkshopDescriptionItem> SetToModel(this IEnumerable<WorkshopDescriptionItemDto> list, IEnumerable<WorkshopDescriptionItem> modelList)
    {
        var result = new List<WorkshopDescriptionItem>(modelList);

        foreach (var item in list)
        {
            var newModelItem = new WorkshopDescriptionItem();
            result.Add(item.SetToModel(newModelItem));
        }

        modelList = result;

        return (List<WorkshopDescriptionItem>)modelList;
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