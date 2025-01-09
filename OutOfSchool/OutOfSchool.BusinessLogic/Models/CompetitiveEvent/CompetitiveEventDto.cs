namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDto : CompetitiveEventBaseDto
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public uint Rating { get; set; } = 0;
    public uint NumberOfRatings { get; set; } = 0;
    public List<CompetitiveEventCoverageDto> Coverage { get; set; }
}