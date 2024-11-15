using OutOfSchool.BusinessLogic.Models.WorkshopDraft;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
public interface IWorkshopDraftService
{
    Task<WorkshopDraftResultDto> Create(WorkshopDraftCreateDto workshopDraftDto);
}
