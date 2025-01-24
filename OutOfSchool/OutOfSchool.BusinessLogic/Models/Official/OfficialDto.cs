using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Official;
public class OfficialDto
{
    public Guid Id { get; set; }
    public string Position { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string Rnokpp { get; set; }
    public string DismissalOrder { get; set; } = string.Empty;
    public string RecruitmentOrder { get; set; } = string.Empty;
    public string DismissalReason { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}
