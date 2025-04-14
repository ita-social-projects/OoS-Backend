using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDto : CompetitiveEventBaseDto
{
    public bool IsDeleted { get; set; }
    public uint Rating { get; set; } = 0;
    public uint NumberOfRatings { get; set; } = 0;
    public string SubDirections { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;
    public IList<string> ImageIds { get; set; }
    public CompetitiveEventCoverageDto Coverage { get; set; }
}