using OutOfSchool.BusinessLogic.Models.Images;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
public class CompetitiveEventResponseDto
{
    public CompetitiveEventV2Dto CompetitiveEventV2 { get; set; }

    public SingleImageUploadingResponse UploadingCoverImageResult { get; set; }

    public MultipleImageUploadingResponse UploadingImagesResults { get; set; }
}
