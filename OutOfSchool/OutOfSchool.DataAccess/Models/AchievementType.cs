using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class AchievementType
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(200)]
        [MinLength(1)]
        public string Title { get; set; }
    }
}
