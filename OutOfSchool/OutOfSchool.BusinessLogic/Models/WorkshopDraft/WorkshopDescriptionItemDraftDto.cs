using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class WorkshopDescriptionItemDraftDto
{
    [Required]
    [MaxLength(200)]
    public string SectionName { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; }
}