using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class PhotosDto
    {
        [Required]
        public long EntityId { get; set; }

        [Required]
        public EntityType EntityType { get; set; }

        public string FileName { get; set; }

        public IFormFileCollection Files { get; set; }
    }
}
