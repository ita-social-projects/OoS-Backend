using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class WorkshopDescriptionItem : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [Required]
    [MinLength(Constants.MinLengthForSectionNameOfWorkshopDescriptionItem)]
    [MaxLength(Constants.MaxLengthForSectionNameOfWorkshopDescriptionItem)]
    public string SectionName { get; set; }

    [Required]
    [MinLength(Constants.MinLengthForDescriptionOfWorkshopDescriptionItem)]
    [MaxLength(Constants.MaxLengthForDescriptionOfWorkshopDescriptionItem)]
    public string Description { get; set; }

    public Guid WorkshopId { get; set; }

    public virtual Workshop Workshop { get; set; }
}