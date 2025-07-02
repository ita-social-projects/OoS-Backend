using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class CompetitiveEventDraftRejectionDto
{
    [Required(ErrorMessage = "Rejection message is required")]
    [MaxLength(Constants.CompetitiveEventDraftMaxRejectionMessageLength)]
    public string RejectionMessage { get; set; }
}
