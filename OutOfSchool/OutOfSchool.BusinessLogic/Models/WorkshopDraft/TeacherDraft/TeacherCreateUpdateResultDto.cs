using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;
public class TeacherCreateUpdateResultDto
{
    public TeacherDraftResponseDto Teacher {  get; set; }
    public Result<string> UploadingAvatarImageResult { get; set; }
}
