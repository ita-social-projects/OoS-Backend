using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Official;
public class OfficialDto
{
    public Guid Id { get; set; }
    public OfficialPositionDto Position { get; set; }
    public OfficialIndividualDto Individual { get; set; }
    public string DismissalOrder { get; set; } = string.Empty;
    public string RecruitmentOrder { get; set; } = string.Empty;
    public string DismissalReason { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}
