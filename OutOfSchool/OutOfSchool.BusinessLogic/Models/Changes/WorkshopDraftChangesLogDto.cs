using WorkshopDraftModel = OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft;

namespace OutOfSchool.BusinessLogic.Models.Changes;

public class WorkshopDraftChangesLogDto : ChangesLogDtoBase
{
    public Guid WorkshopDraftId { get; set; }

    public string WorkshopTitle { get; set; }

    public string ProviderTitle { get; set; }
}

public static class WorkshopDraftChangesLogDtoExtensions
{
    public static WorkshopDraftChangesLogDto ToDto(this ChangesLog changesLog, WorkshopDraftModel draft)
    {
        return new WorkshopDraftChangesLogDto
        {
            FieldName = changesLog.PropertyName,
            OldValue = changesLog.OldValue,
            NewValue = changesLog.NewValue,
            UpdatedDate = DateTime.SpecifyKind(changesLog.UpdatedDate, DateTimeKind.Utc),
            User = changesLog.User?.ToShortUser(),

            WorkshopDraftId = draft.Id,
            WorkshopTitle = draft.WorkshopDraftContent?.Title,
            ProviderTitle = draft.Provider?.FullTitle,

            InstitutionTitle = draft.Provider?.Institution?.Title,
        };
    }
}
