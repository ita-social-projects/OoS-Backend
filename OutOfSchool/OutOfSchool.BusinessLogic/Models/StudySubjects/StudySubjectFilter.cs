namespace OutOfSchool.BusinessLogic.Models.StudySubjects;
public class StudySubjectFilter : SearchStringFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
