using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Responses
{
    public class UpdateWithImagesResponse<TEntity>
    {
        public TEntity UpdatedEntity { get; set; }

        public MultipleImageUploadingResponse UploadingImagesResults { get; set; }

        public MultipleImageRemovingResponse RemovingImagesResults { get; set; }
    }
}
