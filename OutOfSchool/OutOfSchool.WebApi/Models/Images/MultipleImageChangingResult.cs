using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images
{
    public class MultipleImageChangingResult
    {
        public ImageRemovingResult RemovedMultipleResult { get; set; }

        public ImageUploadingResult UploadedMultipleResult { get; set; }
    }
}
