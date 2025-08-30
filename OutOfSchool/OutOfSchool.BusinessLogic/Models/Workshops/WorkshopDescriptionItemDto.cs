using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Services.Models.WorkshopDrafts;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopDescriptionItemDto
{
    public Guid Id { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string SectionName { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(2000)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string Description { get; set; }

    public Guid WorkshopId { get; set; }
}

public static class WorkshopDescriptionItemDtoExtensions
{
    public static WorkshopDescriptionItemDraft ToDraft(this WorkshopDescriptionItemDto dto)
        => new()
        {
            Id = dto.Id == Guid.Empty ? null : dto.Id,
            SectionName = dto.SectionName,
            Description = dto.Description,
        };

    public static List<WorkshopDescriptionItemDraft> ToDraft(this IEnumerable<WorkshopDescriptionItemDto> list)
        => list.MapToList(ToDraft);

    public static WorkshopDescriptionItem ToModel(this WorkshopDescriptionItemDto dto)
        => new()
        {
            Id = dto.Id,
            SectionName = dto.SectionName,
            Description = dto.Description,
            WorkshopId = dto.WorkshopId,
        };

    public static List<WorkshopDescriptionItem> ToModel(this IEnumerable<WorkshopDescriptionItemDto> list)
        => list.MapToList(ToModel);

    public static WorkshopDescriptionItem SetToModel(this WorkshopDescriptionItemDto dto, WorkshopDescriptionItem model)
    {
        model.Id = dto.Id;
        model.SectionName = dto.SectionName;
        model.Description = dto.Description;
        // Do NOT overwrite WorkshopId for existing items to avoid breaking FK when dto.WorkshopId is empty (e.g., from draft)
        if (model.WorkshopId == Guid.Empty && dto.WorkshopId != Guid.Empty)
        {
            model.WorkshopId = dto.WorkshopId;
        }

        return model;
    }

    public static List<WorkshopDescriptionItem> SetToModel(this IEnumerable<WorkshopDescriptionItemDto> dtoList, IEnumerable<WorkshopDescriptionItem> modelList)
    {
        // Prepare lists
        var dtoItems = dtoList?.ToList() ?? new List<WorkshopDescriptionItemDto>();
        var current = modelList?.Where(m => !m.IsDeleted).ToList() ?? new List<WorkshopDescriptionItem>();

        // Build dictionary of incoming items with Ids
        var dtoById = dtoItems.Where(d => d.Id != Guid.Empty).ToDictionary(d => d.Id, d => d);

        // Keep only those existing that are still present in dto (by Id)
        current = current.Where(m => dtoById.ContainsKey(m.Id)).ToList();

        // Update existing
        foreach (var model in current)
        {
            var dto = dtoById[model.Id];
            model.SectionName = dto.SectionName;
            model.Description = dto.Description;
            // Intentionally skip setting model.WorkshopId here to avoid zeroing FK
        }

        // Infer parent WorkshopId for new items
        var inferredWorkshopId = current.FirstOrDefault()?.WorkshopId
                                 ?? dtoItems.FirstOrDefault(d => d.WorkshopId != Guid.Empty)?.WorkshopId
                                 ?? Guid.Empty;

        // Add new items (no Id provided)
        var newDtos = dtoItems.Where(d => d.Id == Guid.Empty);
        foreach (var dto in newDtos)
        {
            var newModel = new WorkshopDescriptionItem();
            dto.SetToModel(newModel);
            if (newModel.WorkshopId == Guid.Empty && inferredWorkshopId != Guid.Empty)
            {
                newModel.WorkshopId = inferredWorkshopId;
            }
            current.Add(newModel);
        }

        return current;
    }

    public static WorkshopDescriptionItemDto ToDto(this WorkshopDescriptionItemDraft model)
        => new()
        {
            Id = model.Id ?? Guid.Empty,
            SectionName = model.SectionName,
            Description = model.Description,
        };

    public static List<WorkshopDescriptionItemDto> ToDto(this IEnumerable<WorkshopDescriptionItemDraft> list)
        => list.MapToList(ToDto);

    public static WorkshopDescriptionItemDto ToDto(this WorkshopDescriptionItem model)
        => new()
        {
            Id = model.Id,
            SectionName = model.SectionName,
            Description = model.Description,
            WorkshopId = model.WorkshopId,
        };

    public static List<WorkshopDescriptionItemDto> ToDto(this IEnumerable<WorkshopDescriptionItem> list)
        => list.MapToList(ToDto);

    public static List<WorkshopDescriptionItemDto> ToNotDeletedDto(this IEnumerable<WorkshopDescriptionItem> list)
        => list.MapNonDeletedToList(ToDto);
}