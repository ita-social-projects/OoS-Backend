using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.CustomResults
{
    public class UpdateResultWithManyImagesDto<TEntity>
    {
        public TEntity Dto { get; set; }

        public ImageUploadingResult UploadingImagesResults { get; set; }

        public ImageRemovingResult RemovingImagesResults { get; set; }
    }
}
