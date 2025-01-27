using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.Services.Enums.WorkshopStatus;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public class WorkshopDraftFilterAdministration : WorkshopFilterAdministration
{
    [EnumDataType(typeof(WorkshopDraftStatus), ErrorMessage = Constants.EnumErrorMessage)]
    [WorkshopDraftStatusValidation]
    public WorkshopDraftStatus WorkshopDraftStatus { get; set; } = WorkshopDraftStatus.PendingModeration;
}
