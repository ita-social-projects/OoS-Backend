using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftResultDto
{
    public WorkshopDraftResponseDto WorkshopDraft { get; set; }

    public OperationResult UploadingCoverImageResult { get; set; }

    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}
