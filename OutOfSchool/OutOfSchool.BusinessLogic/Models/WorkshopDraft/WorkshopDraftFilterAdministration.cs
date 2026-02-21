using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.Services.Enums.WorkshopStatus;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftFilterAdministration : WorkshopFilterAdministration
{
    private HashSet<WorkshopDraftStatus> workshopDraftStatuses;

    [WorkshopDraftStatusValidation]
    public HashSet<WorkshopDraftStatus> WorkshopDraftStatuses
    {
        get => workshopDraftStatuses ?? GetDefaultStatuses();
        set => workshopDraftStatuses = value;
    }

    private static HashSet<WorkshopDraftStatus> GetDefaultStatuses()
    {
        return
        [
            WorkshopDraftStatus.PendingModeration,
            WorkshopDraftStatus.EditedByModerator
        ];
    }
}
