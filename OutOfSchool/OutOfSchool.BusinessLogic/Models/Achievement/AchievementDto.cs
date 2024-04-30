﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.BusinessLogic.Models.Achievement;

namespace OutOfSchool.BusinessLogic.Models;

public class AchievementDto
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

    public List<ChildDto> Children { get; set; }

    public List<AchievementTeacherDto> Teachers { get; set; }
}