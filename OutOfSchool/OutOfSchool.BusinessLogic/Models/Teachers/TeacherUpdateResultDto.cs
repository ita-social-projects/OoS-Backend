using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.Teachers;

public class TeacherUpdateResultDto
{
    public TeacherDTO Teacher { get; set; }

    public OperationResult UploadingAvatarImageResult { get; set; }
}