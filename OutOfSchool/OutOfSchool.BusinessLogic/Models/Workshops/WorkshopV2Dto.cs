using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.SubordinationStructure;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Util.CustomComparers;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopV2Dto : WorkshopDto, IHasCoverImage, IHasImages
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }
}

public static class WorkshopV2DtoExtensions
{
    public static WorkshopDraftContent ToDraftContent(this WorkshopV2Dto dto)
        => new()
        {
            MinAge = dto.MinAge ?? default,
            MaxAge = dto.MaxAge ?? default,
            LanguageOfEducationId = dto.LanguageOfEducationId,
            DateTimeRanges = dto.DateTimeRanges?.ToDraft() ?? [],
            WorkshopDescriptionItems = dto.WorkshopDescriptionItems?.ToDraft() ?? [],
            CompetitiveSelection = dto.CompetitiveSelection,
            ActiveFrom = dto.ActiveFrom,
            ActiveTo = dto.ActiveTo,
            TagIds = (dto.Tags ?? []).Select(x => x.Id).Concat(dto.TagIds ?? []).ToList(),
            Title = dto.Title,
            ProviderTitle = dto.ProviderTitle,
            ProviderTitleEn = dto.ProviderTitleEn,
            IsPaid = dto.IsPaid,
            StudyPeriodStartDate = dto.StudyPeriodDates.StartDate.ToStudyPeriodDate(),
            StudyPeriodEndDate = dto.StudyPeriodDates.EndDate.ToStudyPeriodDate(),
            Keywords = dto.Keywords,
            Address = dto.Address?.ToDraft(),
            OwnershipType = dto.ProviderOwnership,
            AvailableSeats = dto.AvailableSeats ?? default,
            IncludedStudyGroupsIds = dto.IncludedStudyGroups?.Select(x => x.Id).ToList() ?? [],
            ShortTitle = dto.ShortTitle,
            CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
            FormOfLearning = dto.FormOfLearning,
            WorkshopStatus = dto.Status,
            ProviderLicenseStatus = dto.ProviderLicenseStatus,
            Price = dto.IsPaid ? dto.Price ?? default : 0,
            PayRate = dto.PayRate ?? default,
            AreThereBenefits = dto.AreThereBenefits,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            InstitutionId = dto.InstitutionId,
            EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription,
            Coverage = dto.Coverage,
            SpecialNeedsType = SpecialNeedsType.None,
            EducationalShift = EducationalShift.First,
            AgeComposition = AgeComposition.SameAge,
            WorkshopType = dto.WorkshopType,
            ParentWorkshopId = dto.ParentWorkshopId,
            Contacts = dto.Contacts?.ToModel() ?? [],
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            Facebook = dto.Facebook,
            Instagram = dto.Instagram,
        };

    public static OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft SetToDraft(this WorkshopV2Dto dto, OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft model)
    {
        model.ProviderId = dto.ProviderId;
        model.WorkshopId = dto.Id == Guid.Empty ? (Guid?)null : dto.Id;

        return model;
    }

    public static OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft ToDraft(this WorkshopV2Dto dto)
        => new()
        {
            ProviderId = dto.ProviderId,
            WorkshopId = dto.Id == Guid.Empty ? (Guid?)null : dto.Id,
            CoverImageId = dto.CoverImageId,
            WorkshopDraftContent = dto.ToDraftContent(),
            Teachers = dto.Teachers?.ToDraft(),
        };

    public static List<OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft> ToDraft(this IEnumerable<WorkshopV2Dto> list)
        => list.MapToList(ToDraft);

    public static WorkshopV2Dto ToDto(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft)
        => new()
        {
            Id = draft.WorkshopId ?? default,
            Title = draft.WorkshopDraftContent?.Title,
            ShortTitle = draft.WorkshopDraftContent?.ShortTitle,
            MinAge = draft.WorkshopDraftContent?.MinAge,
            MaxAge = draft.WorkshopDraftContent?.MaxAge,
            DateTimeRanges = draft.WorkshopDraftContent?.DateTimeRanges?.ToDto() ?? [],
            IsPaid = draft.WorkshopDraftContent?.IsPaid ?? default,
            Price = draft.WorkshopDraftContent?.Price,
            PayRate = draft.WorkshopDraftContent?.PayRate,
            FormOfLearning = draft.WorkshopDraftContent?.FormOfLearning ?? default,
            AvailableSeats = draft.WorkshopDraftContent?.AvailableSeats ?? default,
            CompetitiveSelection = draft.WorkshopDraftContent?.CompetitiveSelection ?? default,
            CompetitiveSelectionDescription = draft.WorkshopDraftContent?.CompetitiveSelectionDescription,
            WorkshopDescriptionItems = draft.WorkshopDraftContent?.WorkshopDescriptionItems?.ToDto() ?? [],
            InstitutionId = draft.WorkshopDraftContent?.InstitutionId,
            InstitutionHierarchyId = draft.WorkshopDraftContent?.InstitutionHierarchyId,
            DefaultTeacher = draft.Teachers?.FirstOrDefault(x => x.IsDefaultTeacher)?.ToDto() ?? default,
            Keywords = draft.WorkshopDraftContent?.Keywords ?? [],
            Teachers = draft.Teachers?.Where(x => !x.IsDefaultTeacher).ToDto() ?? [],
            ProviderId = draft.ProviderId,
            ProviderTitle = draft.WorkshopDraftContent?.ProviderTitle,
            ProviderTitleEn = draft.WorkshopDraftContent?.ProviderTitleEn,
            ProviderLicenseStatus = draft.WorkshopDraftContent?.ProviderLicenseStatus ?? default,
            ActiveFrom = draft.WorkshopDraftContent?.ActiveFrom ?? default,
            ActiveTo = draft.WorkshopDraftContent?.ActiveTo ?? default,
            IsSelfFinanced = draft.WorkshopDraftContent?.IsSelfFinanced ?? default,
            SpecialNeedsType = draft.WorkshopDraftContent?.SpecialNeedsType ?? default,
            IsInclusive = draft.WorkshopDraftContent?.IsInclusive ?? default,
            EnrollmentProcedureDescription = draft.WorkshopDraftContent?.EnrollmentProcedureDescription,
            AreThereBenefits = draft.WorkshopDraftContent?.AreThereBenefits ?? default,
            PreferentialTermsOfParticipation = draft.WorkshopDraftContent?.PreferentialTermsOfParticipation,
            EducationalShift = draft.WorkshopDraftContent?.EducationalShift ?? default,
            LanguageOfEducationId = draft.WorkshopDraftContent?.LanguageOfEducationId ?? default,
            StudyPeriodDates = draft.WorkshopDraftContent?.ToStudyPeriodDatesDto(),
            AgeComposition = draft.WorkshopDraftContent?.AgeComposition ?? default,
            Coverage = draft.WorkshopDraftContent?.Coverage ?? default,
            WorkshopType = draft.WorkshopDraftContent?.WorkshopType ?? default,
            ParentWorkshopId = draft.WorkshopDraftContent?.ParentWorkshopId,
            Contacts = draft.WorkshopDraftContent?.Contacts?.ToDto() ?? [],
            NoAgeRestrictions = draft.WorkshopDraftContent?.NoAgeRestrictions ?? default,

            CoverImageId = draft.CoverImageId,
            ImageIds = draft.Images?.Select(x => x.ExternalStorageId).ToList() ?? [],
            Status = draft.WorkshopDraftContent?.WorkshopStatus ?? default,
            ProviderOwnership = draft.WorkshopDraftContent?.OwnershipType ?? default,
            Phone = draft.WorkshopDraftContent?.Phone,
            Email = draft.WorkshopDraftContent?.Email,
            Website = draft.WorkshopDraftContent?.Website,
            Facebook = draft.WorkshopDraftContent?.Facebook,
            Instagram = draft.WorkshopDraftContent?.Instagram,
            Address = draft.WorkshopDraftContent?.Address?.ToDto(),            
        };

    public static List<WorkshopV2Dto> ToDto(this IEnumerable<OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft> list)
        => list.MapToList(ToDto);
    
    public static Workshop SetToModel(this WorkshopV2Dto dto, Workshop model)
    {
        model.Id = dto.Id;
        model.Title = dto.Title;
        model.ShortTitle = dto.ShortTitle;
        model.MinAge = dto.MinAge ?? default;
        model.MaxAge = dto.MaxAge ?? default;
        model.DateTimeRanges = dto.DateTimeRanges?.ToModel()
            .Concat(model.DateTimeRanges ?? [])
            .Distinct(new DateTimeRangeComparerWithoutFK())
            .ToList();
        model.IsPaid = dto.IsPaid;
        model.Price = dto.Price ?? default;
        model.PayRate = dto.PayRate ?? default;
        model.FormOfLearning = dto.FormOfLearning;
        model.AvailableSeats = dto.AvailableSeats ?? default;
        model.CompetitiveSelection = dto.CompetitiveSelection;
        model.CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription;
        model.WorkshopDescriptionItems = dto.WorkshopDescriptionItems?.ToModel();
        model.StudyPeriodStartDate = dto.StudyPeriodDates.StartDate.ToStudyPeriodDate();
        model.StudyPeriodEndDate = dto.StudyPeriodDates.EndDate.ToStudyPeriodDate();
        model.InstitutionHierarchyId = dto.InstitutionHierarchyId;
        model.DefaultTeacher = dto.DefaultTeacher?.ToModel(dto.DefaultTeacher.Id, dto.DefaultTeacher.WorkshopId);
        model.Keywords = string.Join(Constants.MappingSeparator, dto.Keywords?.Distinct() ?? []);
        model.ProviderId = dto.ProviderId;
        model.ActiveFrom = dto.ActiveFrom;
        model.ActiveTo = dto.ActiveTo;
        model.IsSelfFinanced = false;
        model.SpecialNeedsType = SpecialNeedsType.None;
        model.IsInclusive = false;
        model.EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription;
        model.AreThereBenefits = dto.AreThereBenefits;
        model.PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation;
        model.EducationalShift = EducationalShift.First;
        model.LanguageOfEducationId = dto.LanguageOfEducationId;
        model.AgeComposition = AgeComposition.SameAge;
        model.Coverage = dto.Coverage;
        model.WorkshopType = dto.WorkshopType;
        model.DefaultTeacherId = dto.DefaultTeacherId;
        model.ParentWorkshopId = dto.ParentWorkshopId;
        model.CoverImageId = dto.CoverImageId;

        return model;
    }

    public static WorkshopV2Dto ToV2Dto(this Workshop model)
    {
        var defaultContact = model.Contacts?.FirstOrDefault(c => c.IsDefault);

        return new()
        {
            Id = model.Id,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            NoAgeRestrictions = model.MinAge == 0 && model.MaxAge == 120,
            MinAge = model.MinAge,
            MaxAge = model.MaxAge,
            DateTimeRanges = model.DateTimeRanges?.ToNotDeletedDto() ?? [],
            IsPaid = model.IsPaid,
            Price = model.Price,
            PayRate = model.PayRate,
            FormOfLearning = model.FormOfLearning,
            AvailableSeats = model.AvailableSeats,
            CompetitiveSelection = model.CompetitiveSelection,
            CompetitiveSelectionDescription = model.CompetitiveSelectionDescription,
            WorkshopDescriptionItems = model.WorkshopDescriptionItems?.ToNotDeletedDto() ?? [],
            InstitutionId = model.InstitutionHierarchy?.InstitutionId,
            Institution = model.InstitutionHierarchy?.Institution.Title,
            InstitutionHierarchyId = model.InstitutionHierarchyId,
            InstitutionHierarchy = model.InstitutionHierarchy?.Title,
            DefaultTeacher = model.DefaultTeacher?.ToDto(),
            DirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(x => !x.IsDeleted).Select(d => d.DirectionId).ToList() ?? [],
            Keywords = model.Keywords?.Split(Constants.MappingSeparator, StringSplitOptions.None),
            Teachers = model.Teachers?.ToNotDeletedDto() ?? [],
            ProviderId = model.ProviderId,
            ProviderTitle = model.ProviderTitle,
            ProviderTitleEn = model.ProviderTitleEn,
            ProviderLicenseStatus = model.Provider?.LicenseStatus ?? default,
            ActiveFrom = model.ActiveFrom,
            ActiveTo = model.ActiveTo,
            IsSelfFinanced = model.IsSelfFinanced,
            SpecialNeedsType = model.SpecialNeedsType,
            IsInclusive = model.IsInclusive,
            EnrollmentProcedureDescription = model.EnrollmentProcedureDescription,
            AreThereBenefits = model.AreThereBenefits,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,
            EducationalShift = model.EducationalShift,
            LanguageOfEducationId = model.LanguageOfEducationId,
            StudyPeriodDates = model.ToStudyPeriodDatesDto(),
            AgeComposition = model.AgeComposition,
            Coverage = model.Coverage,
            WorkshopType = model.WorkshopType,
            DefaultTeacherId = model.DefaultTeacherId,
            ParentWorkshopId = model.ParentWorkshopId,
            ParentWorkshop = model.ParentWorkshop?.ToDto(),
            Contacts = model.Contacts?.ToDto() ?? [],

            TagIds = model.Tags?.Select(x => x.Id).ToList() ?? [],

            CoverImageId = model.CoverImageId,
            ImageIds = model.Images?.Select(x => x.ExternalStorageId).ToList() ?? [],
            Tags = model.Tags?.ToDto() ?? [],
            Status = model.Status,
            IsBlocked = model.Provider?.IsBlocked ?? default,
            ProviderOwnership = model.ProviderOwnership,
            ProviderStatus = model.Provider?.Status ?? default,
            Phone = defaultContact?.Phones?.FirstOrDefault()?.Number,
            Email = defaultContact?.Emails?.FirstOrDefault()?.Address,
            Website = defaultContact?.SocialNetworks?.FirstOrDefault(s => s.Type == SocialNetworkContactType.Website)?.Url,
            Facebook = defaultContact?.SocialNetworks?.FirstOrDefault(s => s.Type == SocialNetworkContactType.Facebook)?.Url,
            Instagram = defaultContact?.SocialNetworks?.FirstOrDefault(s => s.Type == SocialNetworkContactType.Instagram)?.Url,
            Address = defaultContact?.Address?.ToDto(),
        };
    }

    public static List<WorkshopV2Dto> ToV2Dto(this IEnumerable<Workshop> list)
        => list.MapToList(ToV2Dto);
}
