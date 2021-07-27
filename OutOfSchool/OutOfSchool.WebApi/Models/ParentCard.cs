using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ParentCard : WorkshopCard
    {
        [Required]
        public long ChildId { get; set; }

        [Required]
        public long ApplicationId { get; set; }

        public ApplicationStatus Status { get; set; }
    }
}
