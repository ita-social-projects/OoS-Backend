using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.BusinessLogic.Models.Exported.CompetitiveEvents;

public class CoverageInfoDto
{
    public int Id { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }
}

public static class CoverageInfoDtoExtensions
{
    public static CoverageInfoDto ToInfoDto(this CompetitiveEventCoverage model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
        };
}
