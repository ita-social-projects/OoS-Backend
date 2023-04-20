using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class AchievementTypeDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(200)]
    [MinLength(1)]
    public string Title { get; set; }
}