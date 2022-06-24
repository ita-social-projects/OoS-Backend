using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.WebApi.Models.Achievement
{
    public class AchievementCreateDTO
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(2000)]
        [MinLength(1)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime AchievementDate { get; set; } = default;

        [Required]
        public Guid WorkshopId { get; set; }

        [Required]
        public long AchievementTypeId { get; set; }

        public List<Guid> ChildrenIDs { get; set; }

        public List<string> Teachers { get; set; }
    }
}
