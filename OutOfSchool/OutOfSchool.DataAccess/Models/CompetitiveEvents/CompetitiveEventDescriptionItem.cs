using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.CompetitiveEvents;

public class CompetitiveEventDescriptionItem : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Description heading is required")]
    [MaxLength(200)]
    public string SectionName { get; set; }

    [Required(ErrorMessage = "Description text is required")]
    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid? CompetitiveEventId { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }
}
