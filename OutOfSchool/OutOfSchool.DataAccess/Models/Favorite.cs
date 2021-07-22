using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Favorite
    {
        public long Id { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual User User { get; set; }
    }
}
