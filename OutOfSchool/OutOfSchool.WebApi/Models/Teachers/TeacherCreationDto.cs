using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace OutOfSchool.WebApi.Models.Teachers
{
    public class TeacherCreationDto : TeacherDTO
    {
        public IFormFile AvatarImage { get; set; }
    }
}
