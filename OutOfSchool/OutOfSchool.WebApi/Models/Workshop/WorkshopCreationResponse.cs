using System;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Workshop
{
    public class WorkshopCreationResponse
    {
        public Guid WorkshopId { get; set; }

        public MultipleImageUploadingResponse UploadingImagesResults { get; set; }
    }
}
