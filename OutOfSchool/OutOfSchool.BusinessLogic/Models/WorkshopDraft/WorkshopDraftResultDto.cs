using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
/// <summary>
/// DTO representing the result of operations related to workshop draft processing, 
/// including the draft itself, teacher updates, and image upload results.
/// </summary>
public class WorkshopDraftResultDto
{
    /// <summary>
    /// The workshop draft details and its associated data.
    /// </summary>
    public WorkshopDraftResponseDto WorkshopDraft { get; set; }

    /// <summary>
    /// List of results for teacher creation or update operations.
    /// Each entry represents the outcome for an individual teacher.
    /// </summary>
    public List<TeacherCreateUpdateResultDto> TeachersCreateUpdateResult { get; set; }

    /// <summary>
    /// The result of uploading the cover image for the workshop draft.
    /// </summary>
    public OperationResult UploadingCoverImgWorkshopResult { get; set; }

    /// <summary>
    /// The result of uploading multiple images related to the workshop draft.
    /// </summary>
    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}
