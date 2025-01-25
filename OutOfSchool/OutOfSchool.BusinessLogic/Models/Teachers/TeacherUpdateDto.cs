namespace OutOfSchool.BusinessLogic.Models.Teachers;

/// <summary>
/// As teacher logic will be completely re-written later - property re-use and inheritance is ok here.
/// </summary>
public class TeacherUpdateDto : TeacherCreateDto, IHasCoverImage
{
    public Guid Id { get; set; }

    public string CoverImageId { get; set; }
}