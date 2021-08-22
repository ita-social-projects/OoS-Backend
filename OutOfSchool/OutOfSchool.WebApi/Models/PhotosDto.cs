using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OutOfSchool.WebApi.Models
{
    public class PhotosDto
    {
        [Required]
        public long EntityId { get; set; }

        public string FileName { get; set; }

        [Required]
        public IFormFileCollection Files { get; set; }
    }
}
