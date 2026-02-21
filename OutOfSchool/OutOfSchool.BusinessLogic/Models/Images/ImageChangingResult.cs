using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.Images;

public class ImageChangingResult
{
    public OperationResult RemovingResult { get; set; }

    public Result<string> UploadingResult { get; set; }
}