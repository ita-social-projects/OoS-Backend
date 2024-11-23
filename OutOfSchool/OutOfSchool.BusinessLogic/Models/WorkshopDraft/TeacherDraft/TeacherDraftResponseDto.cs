
namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;
public class TeacherDraftResponseDto : TeacherDraftDto
{
    public Guid Id { get; set; }

    public Guid WorkshopDraftId { get; set; }

    public string CoverImageId { get; set; }
}
