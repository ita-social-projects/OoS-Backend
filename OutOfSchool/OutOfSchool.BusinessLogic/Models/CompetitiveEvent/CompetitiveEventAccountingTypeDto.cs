using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventAccountingTypeDto
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }
}

public static class CompetitiveEventAccountingTypeDtoExtensions
{
    public static CompetitiveEventAccountingTypeDto ToDto(this CompetitiveEventAccountingType model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Title = model.Title,
        };

    public static List<CompetitiveEventAccountingTypeDto> ToDto(this IEnumerable<CompetitiveEventAccountingType> list)
        => list.MapToList(ToDto);
}
