using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.CustomResults
{
    public class CreationResultWithManyImagesDto<TKey>
    {
        public TKey Dto { get; set; }

        public ImageUploadingResult UploadingImagesResults { get; set; }
    }
}
