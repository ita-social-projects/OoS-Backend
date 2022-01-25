using System;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Responses
{
    public class CreationWithImagesResponse<TKey>
    {
        public TKey Id { get; set; }

        public MultipleImageUploadingResponse UploadingImagesResults { get; set; }
    }
}
