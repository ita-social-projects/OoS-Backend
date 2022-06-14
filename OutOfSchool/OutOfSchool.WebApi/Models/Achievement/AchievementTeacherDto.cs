using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Achievement
{
    public class AchievementTeacherDto
    {
        public long Id { get; set; }

        [Required]
        public Guid AchievementId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(100)]
        [MinLength(1)]
        public string Title { get; set; }
    }
}
