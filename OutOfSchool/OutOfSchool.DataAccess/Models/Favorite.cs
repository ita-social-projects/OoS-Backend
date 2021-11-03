using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Favorite
    {
        public long Id { get; set; }

        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual User User { get; set; }
    }
}
