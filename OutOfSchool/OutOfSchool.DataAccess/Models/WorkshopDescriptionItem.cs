using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class WorkshopDescriptionItem : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [Required(ErrorMessage = "Description heading is required")]
    [MaxLength(Constants.MaxLengthForSectionNameOfWorkshopDescriptionItem)]
    [MinLength(Constants.MinLengthForSectionNameOfWorkshopDescriptionItem)]
    public string SectionName { get; set; }

    [Required(ErrorMessage = "Description text is required")]
    [MaxLength(Constants.MaxLengthForDescriptionOfWorkshopDescriptionItem)]
    [MinLength(Constants.MinLengthForDescriptionOfWorkshopDescriptionItem)]
    public string Description { get; set; }

    public Guid WorkshopId { get; set; }

    public virtual Workshop Workshop { get; set; }
}