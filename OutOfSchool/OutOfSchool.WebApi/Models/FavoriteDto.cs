using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class FavoriteDto
    {
        public long Id { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Workshop id should be grater than 0")]
        public long WorkshopId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
