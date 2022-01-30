using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Workshop
{
    public class WorkshopCreationResultDto
    {
        public WorkshopDTO Workshop { get; set; }

        public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
    }
}
