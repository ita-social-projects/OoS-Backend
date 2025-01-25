using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.Workshops.V2;

public class WorkshopResultDto
{
    public WorkshopV2Dto Workshop { get; set; }

    public OperationResult UploadingCoverImageResult { get; set; }

    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}