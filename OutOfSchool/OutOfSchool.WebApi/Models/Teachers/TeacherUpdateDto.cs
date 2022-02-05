using Microsoft.AspNetCore.Http;

namespace OutOfSchool.WebApi.Models.Teachers
{
    public class TeacherUpdateDto : TeacherDTO
    {
        public IFormFile ImageFile { get; set; }
    }
}
