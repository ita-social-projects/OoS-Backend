namespace OutOfSchool.BusinessLogic.Models.StudySubjects;
public class StudySubjectDto
{
    public Guid Id { get; set; }
    public string NameInUkrainian { get; set; }
    public string NameInInstructionLanguage { get; set; }
    public bool IsLanguageUkrainian { get; set; }
    public long LanguageId { get; set; }
    public LanguageDto Language { get; set; }
    public Guid WorkshopId { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly ActiveTo { get; set; }
}
