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

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from WorkshopCreateRequestDto
        foreach (var error in base.Validate(validationContext))
            yield return error;

        bool hasCoverImage = CoverImage is { Length: > 0 };
        bool hasCoverImageId = !string.IsNullOrWhiteSpace(CoverImageId);
        bool hasImageFiles = ImageFiles?.Any(f => f is { Length: > 0 }) ?? false;
        bool hasImageIds = ImageIds?.Any(id => !string.IsNullOrWhiteSpace(id)) ?? false;

        bool hasInvalidFiles = ImageFiles?.Any(f => f is null || f.Length == 0) ?? false;
        bool hasInvalidIds = ImageIds?.Any(id => string.IsNullOrWhiteSpace(id)) ?? false;

        if (hasInvalidFiles)
            yield return new ValidationResult("ImageFiles must not contain empty files.", [nameof(ImageFiles)]);
        if (hasInvalidIds)
            yield return new ValidationResult("ImageIds must not contain empty or whitespace strings.", [nameof(ImageIds)]);

        if (hasCoverImage == hasCoverImageId)
        {
            yield return new ValidationResult("Either CoverImage or CoverImageId should be provided, not both.",
                [nameof(CoverImage), nameof(CoverImageId)]);
        }

        if (!hasImageFiles && !hasImageIds)
        {
            yield return new ValidationResult("At least one of the ImageFiles or ImageIds fields must be filled in.",
                [nameof(ImageFiles), nameof(ImageIds)]);
        }

        if ((ImageFiles ?? []).Count + (ImageIds ?? []).Count > Constants.MaxCountOfImagesForWorkshop)
        {
            yield return new ValidationResult($"A maximum of {Constants.MaxCountOfImagesForWorkshop} images are allowed for a workshop.",
                [nameof(ImageFiles), nameof(ImageIds)]);
        }
    }
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
            DateTimeRanges = dto.DateTimeRanges?.ToModel() ?? [],
            FormOfLearning = dto.FormOfLearning,
            CompetitiveSelection = dto.CompetitiveSelection,
            CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
            ProviderId = dto.ProviderId,

            WorkshopType = dto.WorkshopType,
            ParentWorkshopId = dto.ParentWorkshopId,
            IsPaid = dto.IsPaid,
            StudyPeriodStartDate = dto.StudyPeriodDates.StartDate.ToStudyPeriodDate(),
            StudyPeriodEndDate = dto.StudyPeriodDates.EndDate.ToStudyPeriodDate(),
            Price = dto.IsPaid ? dto.Price ?? 0 : 0,
            PayRate = dto.PayRate ?? default,
            AreThereBenefits = dto.AreThereBenefits,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,

            WorkshopDescriptionItems = dto.WorkshopDescriptionItems?.ToModel() ?? [],
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            Keywords = string.Join(Constants.MappingSeparator, dto.Keywords?.Distinct() ?? []),
            EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription,
            Coverage = dto.Coverage,

            DefaultTeacher = dto.DefaultTeacher?.ToModel(),
            DefaultTeacherId = dto.DefaultTeacherId,
            
            AvailableSeats = dto.AvailableSeats ?? default,
            Contacts = dto.Contacts?.ToModel() ?? [],
            LanguageOfEducationId = dto.LanguageOfEducationId,
            IsChampionPath = dto.IsChampionPath,
            // If we're converting from draft, we need to keep cover image
            CoverImageId = dto.CoverImageId.IsNullOrEmpty() ? null : dto.CoverImageId,
            MinsportSectionId = dto.MinsportSectionId,
        };

    public static WorkshopV2CreateRequestDto ToV2CreateRequestDto(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft)
        => new()
        {
            Title = draft.WorkshopDraftContent?.Title,
            ShortTitle = draft.WorkshopDraftContent?.ShortTitle,
            MinAge = draft.WorkshopDraftContent?.MinAge,
            MaxAge = draft.WorkshopDraftContent?.MaxAge,
            DateTimeRanges = draft.WorkshopDraftContent?.DateTimeRanges?.ToDto() ?? [],
            FormOfLearning = draft.WorkshopDraftContent?.FormOfLearning ?? default,
            AvailableSeats = draft.WorkshopDraftContent?.AvailableSeats,
            CompetitiveSelection = draft.WorkshopDraftContent?.CompetitiveSelection ?? default,
            CompetitiveSelectionDescription = draft.WorkshopDraftContent?.CompetitiveSelectionDescription,
            ProviderId = draft.ProviderId,

            IsSelfFinanced = draft.WorkshopDraftContent?.IsSelfFinanced ?? default,
            IsInclusive = draft.WorkshopDraftContent?.IsInclusive ?? default,
            EducationalShift = draft.WorkshopDraftContent?.EducationalShift ?? default,
            LanguageOfEducationId = draft.WorkshopDraftContent?.LanguageOfEducationId ?? default,
            StudyPeriodDates = draft.WorkshopDraftContent?.ToStudyPeriodDatesDto(),
            AgeComposition = draft.WorkshopDraftContent?.AgeComposition ?? default,
            WorkshopType = draft.WorkshopDraftContent?.WorkshopType ?? default,
            ParentWorkshopId = draft.WorkshopDraftContent?.ParentWorkshopId,
            IsPaid = draft.WorkshopDraftContent?.IsPaid ?? default,
            Price = draft.WorkshopDraftContent?.Price,
            PayRate = draft.WorkshopDraftContent?.PayRate,
            AreThereBenefits = draft.WorkshopDraftContent?.AreThereBenefits ?? default,
            PreferentialTermsOfParticipation = draft.WorkshopDraftContent?.PreferentialTermsOfParticipation,

            WorkshopDescriptionItems = draft.WorkshopDraftContent?.WorkshopDescriptionItems?.ToDto() ?? [],
            InstitutionId = draft.WorkshopDraftContent?.InstitutionId,
            InstitutionHierarchyId = draft.WorkshopDraftContent?.InstitutionHierarchyId,
            Keywords = draft.WorkshopDraftContent?.Keywords ?? [],
            EnrollmentProcedureDescription = draft.WorkshopDraftContent?.EnrollmentProcedureDescription,
            Coverage = draft.WorkshopDraftContent?.Coverage ?? default,
            TagIds = draft.WorkshopDraftContent?.TagIds ?? [],

            Contacts = draft.WorkshopDraftContent?.Contacts?.ToDto() ?? [],

            DefaultTeacher = draft.Teachers?.FirstOrDefault(x => x.IsDefaultTeacher)?.ToDto(),
            Teachers = draft.Teachers?.Where(x => !x.IsDefaultTeacher).ToDto() ?? [],

            CoverImageId = draft.CoverImageId,
            ImageIds = draft.Images?.Select(i => i.ExternalStorageId).ToList() ?? [],
            IsChampionPath = draft.WorkshopDraftContent?.IsChampionPath ?? default,
            MinsportSectionId = draft.WorkshopDraftContent?.MinsportSectionId,
        };
}