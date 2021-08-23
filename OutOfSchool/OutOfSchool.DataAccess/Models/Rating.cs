using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Rating
    {
        public long Id { get; set; }

        [Range(1, 5)]
        public int Rate { get; set; }

        [Required]
        [Range(1, 2, ErrorMessage = "The type field should be 1 or 2")]
        public RatingType Type { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "The EntityId field should be bigger than 0")]
        public long EntityId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "The ParentId field should be bigger than 0")]
        public long ParentId { get; set; }

        [Required]
        public DateTimeOffset CreationTime { get; set; }

        public virtual Parent Parent { get; set; }
    }
}
