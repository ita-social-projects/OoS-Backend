using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Rating
    {
        public long Id { get; set; }

        public int Rate { get; set; } = default;

        [Required]
        public RatingType Type { get; set; }

        [Required]
        public long EntityId { get; set; }

        [Required]
        public virtual Parent Parent { get; set; }
    }
}
