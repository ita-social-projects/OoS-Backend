using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopV2CreateRequestDto : WorkshopCreateRequestDto
{
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }
}

public static class WorkshopV2CreateRequestDtoExtensions
{
    public static Workshop ToModel(this WorkshopV2CreateRequestDto dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            MinAge = dto.MinAge ?? default,
            MaxAge = dto.MaxAge ?? default,
            DateTimeRanges = dto.DateTimeRanges.ToModel(),
            FormOfLearning = dto.FormOfLearning,
            CompetitiveSelection = dto.CompetitiveSelection,
            CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
            ProviderId = dto.ProviderId,

            WorkshopType = dto.WorkshopType,
            ParentWorkshopId = dto.ParentWorkshopId,
            IsPaid = dto.IsPaid,
            Price = dto.IsPaid ? dto.Price ?? 0 : 0,
            PayRate = dto.PayRate ?? default,
            AreThereBenefits = dto.AreThereBenefits,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,

            WorkshopDescriptionItems = dto.WorkshopDescriptionItems.ToModel(),
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            Keywords = string.Join(Constants.MappingSeparator, dto.Keywords.Distinct()),
            EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription,
            Coverage = dto.Coverage,

            DefaultTeacher = dto.DefaultTeacher.ToModel(),
            DefaultTeacherId = dto.DefaultTeacherId,
        };

    public static WorkshopV2CreateRequestDto ToV2CreateRequestDto(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft)
        => new()
        {
            Title = draft.WorkshopDraftContent.Title,
            ShortTitle = draft.WorkshopDraftContent.ShortTitle,
            MinAge = draft.WorkshopDraftContent.MinAge,
            MaxAge = draft.WorkshopDraftContent.MaxAge,
            DateTimeRanges = draft.WorkshopDraftContent.DateTimeRanges.ToDto(),
            FormOfLearning = draft.WorkshopDraftContent.FormOfLearning,
            AvailableSeats = draft.WorkshopDraftContent.AvailableSeats,
            CompetitiveSelection = draft.WorkshopDraftContent.CompetitiveSelection,
            CompetitiveSelectionDescription = draft.WorkshopDraftContent.CompetitiveSelectionDescription,
            ProviderId = draft.ProviderId,

            IsSelfFinanced = draft.WorkshopDraftContent.IsSelfFinanced,
            IsInclusive = draft.WorkshopDraftContent.IsInclusive,
            EducationalShift = draft.WorkshopDraftContent.EducationalShift,
            LanguageOfEducationId = draft.WorkshopDraftContent.LanguageOfEducationId,
            AgeComposition = draft.WorkshopDraftContent.AgeComposition,
            WorkshopType = draft.WorkshopDraftContent.WorkshopType,
            ParentWorkshopId = draft.WorkshopDraftContent.ParentWorkshopId,
            IsPaid = draft.WorkshopDraftContent.IsPaid,
            Price = draft.WorkshopDraftContent.Price,
            PayRate = draft.WorkshopDraftContent.PayRate,
            AreThereBenefits = draft.WorkshopDraftContent.AreThereBenefits,
            PreferentialTermsOfParticipation = draft.WorkshopDraftContent.PreferentialTermsOfParticipation,

            WorkshopDescriptionItems = draft.WorkshopDraftContent.WorkshopDescriptionItems.ToDto(),
            InstitutionId = draft.WorkshopDraftContent.InstitutionId,
            InstitutionHierarchyId = draft.WorkshopDraftContent.InstitutionHierarchyId,
            Keywords = draft.WorkshopDraftContent.Keywords,
            EnrollmentProcedureDescription = draft.WorkshopDraftContent.EnrollmentProcedureDescription,
            Coverage = draft.WorkshopDraftContent.Coverage,
            TagIds = draft.WorkshopDraftContent.TagIds,

            Contacts = draft.WorkshopDraftContent.Contacts.ToDto(),

            DefaultTeacher = draft.Teachers.FirstOrDefault(x => x.IsDefaultTeacher)?.ToDto(),
            Teachers = draft.Teachers.Where(x => !x.IsDefaultTeacher).ToDto(),

            CoverImageId = draft.CoverImageId
        };
}