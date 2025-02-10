using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class WorkshopDescriptionItemDraftDto
{
    [Required]
    [MaxLength(Constants.WorkshopDraftDescriptionItemsLength)]
    public string SectionName { get; set; }

    [Required]
    [MaxLength(Constants.WorkshopDraftDescriptionItemsLength)]
    public string Description { get; set; }
}