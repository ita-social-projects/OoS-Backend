using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.CustomResults
{
    public class UpdateResultWithImageDto<TKey>
    {
        public TKey Dto { get; set; }

        public OperationResult UploadingImageResult { get; set; }

        public OperationResult RemovingImageResult { get; set; }
    }
}
