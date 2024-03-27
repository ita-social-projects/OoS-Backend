using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Workshops;

public class WorkshopResponseDto
{
    public WorkshopV2Dto Workshop { get; set; }

    public SingleImageUploadingResponse UploadingCoverImageResult { get; set; }

    public MultipleImageUploadingResponse UploadingImagesResults { get; set; }
}