using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class ParentCardDto : CardDto
    {
        [Required]
        public long ChildId { get; set; }

        public int Status { get; set; }
    }
}
