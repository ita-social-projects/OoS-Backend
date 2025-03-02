using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftRejectionDto
{
    [Required(ErrorMessage = "Rejection message is required")]
    [MaxLength(Constants.WorkshopDraftMaxRejectionMessageLength)]
    public string RejectionMessage { get; set; }
}
