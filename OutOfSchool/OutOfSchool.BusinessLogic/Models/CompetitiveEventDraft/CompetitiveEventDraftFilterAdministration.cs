using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;

public class CompetitiveEventDraftFilterAdministration : CompetitiveEventFilterAdministration
{
    private HashSet<CompetitiveEventDraftStatus> competitiveEventDraftStatuses;

    [CompetitiveEventDraftStatusValidation]
    public HashSet<CompetitiveEventDraftStatus> CompetitiveEventDraftStatuses
    {
        get => competitiveEventDraftStatuses ?? GetDefaultStatuses();
        set => competitiveEventDraftStatuses = value;
    }

    private static HashSet<CompetitiveEventDraftStatus> GetDefaultStatuses()
    {
        return
        [
            CompetitiveEventDraftStatus.PendingModeration,
            CompetitiveEventDraftStatus.EditedByModerator
        ];
    }
} 