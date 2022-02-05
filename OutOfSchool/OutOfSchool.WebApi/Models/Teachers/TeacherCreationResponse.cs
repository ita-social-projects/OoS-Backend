using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Models.Teachers
{
    public class TeacherCreationResponse
    {
        public TeacherDTO Teacher { get; set; }

        public MultipleImageUploadingResponse UploadingAvatarImageResult { get; set; }
    }
}
