using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class AchievementType : IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(200)]
    [MinLength(1)]
    public string Title { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(200)]
    [MinLength(1)]
    public string TitleEn { get; set; }

    public bool IsDeleted { get; set; }
}