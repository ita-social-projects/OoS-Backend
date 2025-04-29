using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventCoverageDto
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string TitleEn { get; set; }
}

public static class CompetitiveEventCoverageDtoExtensions
{
    public static CompetitiveEventCoverageDto ToDto(this CompetitiveEventCoverage model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Title = model.Title,
            TitleEn = model.TitleEn,
        };

    public static List<CompetitiveEventCoverageDto> ToDto(this IEnumerable<CompetitiveEventCoverage> list)
        => list.MapToList(ToDto);
}
