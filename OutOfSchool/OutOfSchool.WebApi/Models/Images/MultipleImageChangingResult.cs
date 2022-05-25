using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images
{
    public class MultipleImageChangingResult
    {
        public MultipleImageRemovingResult RemovedMultipleResult { get; set; }

        public MultipleImageUploadingResult UploadedMultipleResult { get; set; }
    }
}
