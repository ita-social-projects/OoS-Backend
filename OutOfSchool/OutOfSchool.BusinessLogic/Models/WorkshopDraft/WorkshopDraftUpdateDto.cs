using OutOfSchool.BusinessLogic.Models.Workshops;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class WorkshopDraftUpdateDto
{
    [Required(ErrorMessage = "WorkshopDraftId is required")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "WorkshopV2Dto is required")]
    public WorkshopV2Dto WorkshopV2Dto { get; set; }
}

