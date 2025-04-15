using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
public class CompetitiveEventResultDto
{
    public CompetitiveEventV2Dto CompetitiveEventV2 { get; set; }

    public OperationResult UploadingCoverImageResult { get; set; }

    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}
