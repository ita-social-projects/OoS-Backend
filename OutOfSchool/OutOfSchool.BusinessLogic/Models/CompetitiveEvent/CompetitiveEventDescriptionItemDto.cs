using OutOfSchool.Services.Models.CompetitiveEvents;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDescriptionItemDto
{
    public Guid Id { get; set; }

    [Required]
    [MinLength(Constants.MinLengthForSectionNameOfCompetitiveEventDescriptionItem)]
    [MaxLength(Constants.MaxLengthForSectionNameOfCompetitiveEventDescriptionItem)]
    public string SectionName { get; set; }

    [Required]
    [MinLength(Constants.MinLengthForDescriptionOfCompetitiveEventDescriptionItem)]
    [MaxLength(Constants.MaxLengthForDescriptionOfCompetitiveEventDescriptionItem)]
    public string Description { get; set; }

    public Guid CompetitiveEventId { get; set; }
}

public static class CompetitiveEventDescriptionItemDtoExtensions
{
    public static CompetitiveEventDescriptionItem ToDraft(this CompetitiveEventDescriptionItemDto dto) => new()
    {
        Id = dto.Id,
        SectionName = dto.SectionName,
        Description = dto.Description,
        CompetitiveEventId = dto.CompetitiveEventId,
    };
    
    public static List<CompetitiveEventDescriptionItem> ToDraft(this IEnumerable<CompetitiveEventDescriptionItemDto> list)
        => list.MapToList(ToDraft);

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
            CompetitiveEventId = dto.CompetitiveEventId
        };

    public static List<CompetitiveEventDescriptionItem> ToModel(this IEnumerable<CompetitiveEventDescriptionItemDto> list)
        => list.MapToList(ToModel);

    public static CompetitiveEventDescriptionItemDto ToDto(this CompetitiveEventDescriptionItem model)
        => new()
        {
            Id = model.Id,
            SectionName = model.SectionName,
            Description = model.Description,
            CompetitiveEventId = model.CompetitiveEventId ?? Guid.Empty
        };

    public static List<CompetitiveEventDescriptionItemDto> ToDto(this IEnumerable<CompetitiveEventDescriptionItem> list)
        => list.MapToList(ToDto);    
}
