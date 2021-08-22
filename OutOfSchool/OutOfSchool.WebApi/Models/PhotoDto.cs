using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class PhotoDto
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        [Required]
        public long EntityId { get; set; }

        public IFormFile File { get; set; }
    }
}
