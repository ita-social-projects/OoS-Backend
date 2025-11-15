using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.CompetitiveEvents;

public class CompetitiveEventDescriptionItem : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [Required]
    [MinLength(Constants.MinLengthForSectionNameOfCompetitiveEventDescriptionItem)]
    [MaxLength(Constants.MaxLengthForSectionNameOfCompetitiveEventDescriptionItem)]
    public string SectionName { get; set; }

    [Required]
    [MinLength(Constants.MinLengthForDescriptionOfCompetitiveEventDescriptionItem)]
    [MaxLength(Constants.MaxLengthForDescriptionOfCompetitiveEventDescriptionItem)]
    public string Description { get; set; }

    public Guid? CompetitiveEventId { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }
}
