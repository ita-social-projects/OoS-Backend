using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDescriptionItemDto
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SectionName { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid CompetitiveEventId { get; set; }
}

public static class CompetitiveEventDescriptionItemDtoExtensions
{
    public static CompetitiveEventDescriptionItem SetToModel(this CompetitiveEventDescriptionItemDto dto, CompetitiveEventDescriptionItem model)
    {
        model.SectionName = dto.SectionName;
        model.Description = dto.Description;
        model.CompetitiveEventId = dto.CompetitiveEventId;

        return model;
    }

    public static CompetitiveEventDescriptionItem ToModel(this CompetitiveEventDescriptionItemDto dto)
        => new()
        { 
            Id = dto.Id,
            SectionName = dto.SectionName,
            Description = dto.Description,
        };

    public static List<CompetitiveEventDescriptionItem> ToModel(this IEnumerable<CompetitiveEventDescriptionItemDto> list)
        => list.MapToList(ToModel);

    public static CompetitiveEventDescriptionItemDto ToDto(this CompetitiveEventDescriptionItem dto)
        => new()
        {
            Id = dto.Id,
            SectionName = dto.SectionName,
            Description = dto.Description,
            CompetitiveEventId = dto.CompetitiveEventId ?? default
        };

    public static List<CompetitiveEventDescriptionItemDto> ToDto(this IEnumerable<CompetitiveEventDescriptionItem> list)
        => list.MapToList(ToDto);
}
