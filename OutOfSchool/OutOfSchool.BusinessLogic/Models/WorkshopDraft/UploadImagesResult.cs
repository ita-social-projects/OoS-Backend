using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDraft;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
/// <summary>
/// Represents the result of uploading workshop and teacher images.
/// </summary>
public class UploadImagesResult
{
    /// <summary>
    /// Results of uploading images for teachers.
    /// </summary>
    public List<TeacherCreateUpdateResultDto> TeacherImagesUploadingResults { get; set; }

    /// <summary>
    /// Results of uploading workshop images.
    /// </summary>
    public MultipleImageUploadingResult WorkshopImagesUploadingResult { get; set; }

    /// <summary>
    /// Result of uploading the workshop cover image.
    /// </summary>
    public OperationResult WorkshopCoverImageUploadingResult { get; set; }
}
