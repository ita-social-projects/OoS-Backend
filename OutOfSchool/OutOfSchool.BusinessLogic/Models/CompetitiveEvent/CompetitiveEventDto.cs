namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDto : CompetitiveEventBaseDto
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public List<CompetitiveEventCoverageDto> Coverage { get; set; }
}