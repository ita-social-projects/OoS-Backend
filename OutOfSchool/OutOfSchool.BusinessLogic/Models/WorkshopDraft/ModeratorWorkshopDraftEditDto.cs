using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using System.ComponentModel.DataAnnotations;
using WorkshopDraftModel = OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

/// <summary>
/// DTO for editing workshop drafts by moderators. Contains only fields allowed to be modified by a moderator.
/// </summary>
public class ModeratorWorkshopDraftEditDto
{
    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CompetitiveSelectionDescription { get; set; }

    [MaxLength(500)]
    public string PreferentialTermsOfParticipation { get; set; }

    [MaxLength(500)]
    public string EnrollmentProcedureDescription { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description item is required")]
    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }
}

public static class ModeratorWorkshopDraftEditDtoExtensions
{
    public static WorkshopDraftModel ToDraft(this ModeratorWorkshopDraftEditDto dto, WorkshopDraftModel model)
    {
        model.WorkshopDraftContent.Title = dto.Title;
        model.WorkshopDraftContent.ShortTitle = dto.ShortTitle;
        model.WorkshopDraftContent.CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription;
        model.WorkshopDraftContent.PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation;
        model.WorkshopDraftContent.EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription;
        model.WorkshopDraftContent.InstitutionHierarchyId = dto.InstitutionHierarchyId;
        model.WorkshopDraftContent.WorkshopDescriptionItems = dto.WorkshopDescriptionItems.ToDraft();

        return model;
    }
}
