using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class AchievementTeacher : IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }

    [Required]
    public Guid AchievementId { get; set; }

    public virtual Achievement Achievement { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Title { get; set; }
}