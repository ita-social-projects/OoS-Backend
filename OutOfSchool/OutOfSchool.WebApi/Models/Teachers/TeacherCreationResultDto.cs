using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Teachers;

public class TeacherCreationResultDto
{
    public TeacherDTO Teacher { get; set; }

    public OperationResult UploadingAvatarImageResult { get; set; }
}