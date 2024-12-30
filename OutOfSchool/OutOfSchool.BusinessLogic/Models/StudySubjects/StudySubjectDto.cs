namespace OutOfSchool.BusinessLogic.Models.StudySubjects;
public class StudySubjectDto
{
    public Guid Id { get; set; }
    public string NameInUkrainian { get; set; }
    public string NameInInstructionLanguage { get; set; }
    public bool IsPrimaryLanguageUkrainian { get; set; }
    public List<Language> Languages { get; set; }
    public int PrimaryLanguageId { get; set; }
    public Guid WorkshopId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}
