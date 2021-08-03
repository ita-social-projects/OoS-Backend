using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Photo
    {
        public long Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public long EntityId { get; set; }

        [Required]
        public EntityType EntityType { get; set; }
    }
}
