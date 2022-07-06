using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models;

public class Achievement : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [Required]
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

    public virtual Workshop Workshop { get; set; }

    [Required]
    public long AchievementTypeId { get; set; }

    public virtual AchievementType AchievementType { get; set; }

    public virtual List<Child> Children { get; set; }

    public virtual List<AchievementTeacher> Teachers { get; set; }
}