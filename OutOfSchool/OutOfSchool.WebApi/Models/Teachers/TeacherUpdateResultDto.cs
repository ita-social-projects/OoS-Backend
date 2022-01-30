using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Teachers
{
    public class TeacherUpdateResultDto
    {
        public TeacherDTO Teacher { get; set; }

        public OperationResult UploadingAvatarImageResult { get; set; }

        public OperationResult RemovingAvatarImageResult { get; set; }
    }
}
