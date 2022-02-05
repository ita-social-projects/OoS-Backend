using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images
{
    public class ImageChangingResult
    {
        public OperationResult RemovingResult { get; set; }

        public Result<string> UploadingResult { get; set; }
    }
}
