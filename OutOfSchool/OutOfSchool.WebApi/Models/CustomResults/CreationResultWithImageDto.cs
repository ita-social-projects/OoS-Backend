using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.CustomResults
{
    public class CreationResultWithImageDto<TKey>
    {
        public TKey Dto { get; set; }

        public OperationResult UploadingImageResult { get; set; }
    }
}
