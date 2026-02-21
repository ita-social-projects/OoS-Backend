using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Images;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
/// <summary>
/// represents the result of uploading images for a competitive event draft.
/// </summary>
public class UploadCompetitiveEventDraftImagesResult
{
    /// <summary>
    /// The result of uploading the cover image for the competitive event draft.
    /// </summary>
    public OperationResult UploadingCoverImageResult { get; set; }

    /// <summary>
    /// The result of uploading multiple images related to the competitive event draft.
    /// </summary>
    public MultipleImageUploadingResult UploadingImagesResults { get; set; }
}
