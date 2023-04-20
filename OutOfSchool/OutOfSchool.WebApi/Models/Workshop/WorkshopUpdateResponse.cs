using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopUpdateResponse
{
    public WorkshopDTO Workshop { get; set; }

    public SingleImageUploadingResponse UploadingCoverImageResult { get; set; }

    public MultipleImageUploadingResponse UploadingImagesResults { get; set; }
}