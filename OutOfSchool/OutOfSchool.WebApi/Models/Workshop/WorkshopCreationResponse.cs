using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Workshop
{
    public class WorkshopCreationResponse
    {
        public WorkshopDTO Workshop { get; set; }

        public MultipleImageUploadingResponse UploadingImagesResults { get; set; }
    }
}
