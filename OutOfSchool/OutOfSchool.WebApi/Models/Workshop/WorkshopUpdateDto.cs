using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Models.Teachers;

namespace OutOfSchool.WebApi.Models.Workshop
{
    public class WorkshopUpdateDto : WorkshopDTO
    {
        public List<IFormFile> ImageFiles { get; set; }

        public IFormFile CoverImage { get; set; }

        public new List<TeacherUpdateDto> Teachers { get; set; }
    }
}
