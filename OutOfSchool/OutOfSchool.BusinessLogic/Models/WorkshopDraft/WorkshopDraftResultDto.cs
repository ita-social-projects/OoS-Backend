using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftResultDto
{
    public WorkshopDraftResponseDto WorkshopDraft { get; set; }

    public List<TeacherCreateUpdateResultDto> TeacherCreateUpdateResut {  get; set; }

    public OperationResult UploadingCoverImgWorkshopResult { get; set; }

    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}
