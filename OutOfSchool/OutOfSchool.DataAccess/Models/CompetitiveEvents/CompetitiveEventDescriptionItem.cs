using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.CompetitiveEvents;

public class CompetitiveEventDescriptionItem : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Description heading is required")]
    [MaxLength(Constants.MaxLengthForSectionNameOfCompetitiveEventDescriptionItem)]
    [MinLength(Constants.MinLengthForSectionNameOfCompetitiveEventDescriptionItem)]
    public string SectionName { get; set; }

    [Required(ErrorMessage = "Description text is required")]
    [MaxLength(Constants.MaxLengthForDescriptionOfCompetitiveEventDescriptionItem)]
    [MinLength(Constants.MinLengthForDescriptionOfCompetitiveEventDescriptionItem)]
    public string Description { get; set; }

    public Guid? CompetitiveEventId { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }
}
