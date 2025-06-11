using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class CompetitiveEventDraftResultDto
{
    /// <summary>
    /// The competitive event draft details and its associated data.
    /// </summary>
    public CompetitiveEventDraftResponseDto CompetitiveEventDraft { get; set; }

    /// <summary>
    /// The result of uploading the cover image for the workshop draft.
    /// </summary>
    public OperationResult UploadingCoverImagesCompetitiveEventResult { get; set; }

    /// <summary>
    /// The result of uploading multiple images related to the workshop draft.
    /// </summary>
    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}
