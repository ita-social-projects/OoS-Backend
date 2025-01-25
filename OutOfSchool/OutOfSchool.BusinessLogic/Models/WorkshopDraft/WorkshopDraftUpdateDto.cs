using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Workshops.V2;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

public class WorkshopDraftUpdateDto
{
    [Required(ErrorMessage = "WorkshopDraftId is required")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "WorkshopV2Dto is required")]
    public WorkshopUpdateV2Dto WorkshopV2Dto { get; set; }
}

