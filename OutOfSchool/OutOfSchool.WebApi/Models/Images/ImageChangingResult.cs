using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images
{
    public class ImageChangingResult
    {
        public MultipleKeyValueOperationResult RemovedMultipleResult { get; set; }

        public MultipleKeyValueOperationResult UploadedMultipleResult { get; set; }
    }
}
