using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;
/// <summary>
/// Represents the result of a teacher creation or update operation, 
/// including details about the teacher and the result of the cover image upload.
/// </summary>
public class TeacherCreateUpdateResultDto
{
    /// <summary>
    /// Contains the details of the created or updated teacher.
    /// </summary>
    public TeacherDraftResponseDto Teacher { get; set; }

    /// <summary>
    /// Represents the result of the operation to upload the teacher's cover image,
    /// including success or failure information.
    /// </summary>
    public OperationResult UploadingCoverImageResult { get; set; }
}
