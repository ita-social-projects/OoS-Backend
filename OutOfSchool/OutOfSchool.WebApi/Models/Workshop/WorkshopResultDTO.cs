using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopResultDTO
{
    public WorkshopV2DTO Workshop { get; set; }

    public OperationResult UploadingCoverImageResult { get; set; }

    public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
}