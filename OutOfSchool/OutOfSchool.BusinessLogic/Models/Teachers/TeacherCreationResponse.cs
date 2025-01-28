using OutOfSchool.BusinessLogic.Models.Images;

namespace OutOfSchool.BusinessLogic.Models.Teachers;

public class TeacherCreationResponse
{
    public TeacherDto Teacher { get; set; }

    public SingleImageUploadingResponse UploadingAvatarImageResult { get; set; }
}