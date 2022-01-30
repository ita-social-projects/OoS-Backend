using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Workshop
{
    public class WorkshopUpdateResponse
    {
        public WorkshopDTO Workshop { get; set; }

        public MultipleImageUploadingResponse UploadingImagesResults { get; set; }

        public MultipleImageRemovingResponse RemovingImagesResults { get; set; }
    }
}
