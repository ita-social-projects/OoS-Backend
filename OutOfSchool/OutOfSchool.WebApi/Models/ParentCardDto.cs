using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ParentCardDto : CardDto
    {
        [Required]
        public long ChildId { get; set; }

        public ApplicationStatus Status { get; set; }
    }
}
