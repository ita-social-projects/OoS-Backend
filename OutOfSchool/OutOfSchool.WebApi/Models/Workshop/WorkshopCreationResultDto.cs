using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopCreationResultDto
{
    public WorkshopDTO Workshop { get; set; }

    public OperationResult UploadingCoverImageResult { get; set; }

    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}