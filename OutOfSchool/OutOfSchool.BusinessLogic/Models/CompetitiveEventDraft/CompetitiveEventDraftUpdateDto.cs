using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class CompetitiveEventDraftUpdateDto
{
    [Required(ErrorMessage = "CompetitiveEventDRaftId is required")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "CompetitiveEventV2Dto is required")]
    public CompetitiveEventV2Dto CompetitiveEventV2Dto { get; set; }
}
