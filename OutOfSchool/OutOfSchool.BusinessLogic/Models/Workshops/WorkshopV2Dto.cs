using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Util.CustomComparers;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

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
    private static readonly char[] TrimChars = { ' ', ',', '.',';',':' };
    public static WorkshopDraftContent ToDraftContent(this WorkshopV2Dto dto)
        => new()
        {
            MinAge = dto.MinAge ?? default,
            MaxAge = dto.MaxAge ?? default,
            LanguageOfEducationId = dto.LanguageOfEducationId,
            LanguageOfEducationName = dto.LanguageOfEducationName,
            DateTimeRanges = dto.DateTimeRanges?.ToDraft() ?? [],
            WorkshopDescriptionItems = dto.WorkshopDescriptionItems?.ToDraft() ?? [],
            CompetitiveSelection = dto.CompetitiveSelection,
            ActiveFrom = dto.ActiveFrom,
            ActiveTo = dto.ActiveTo,
            TagIds = (dto.Tags ?? []).Select(x => x.Id).Concat(dto.TagIds ?? []).ToList(),
            Title = dto.Title.Trim(TrimChars),
            IsPaid = dto.IsPaid,
            StudyPeriodStartDate = dto.StudyPeriodDates.StartDate.ToStudyPeriodDate(),
            StudyPeriodEndDate = dto.StudyPeriodDates.EndDate.ToStudyPeriodDate(),
            Keywords = dto.Keywords,
            OwnershipType = dto.ProviderOwnership,
            AvailableSeats = dto.AvailableSeats ?? default,
            IncludedStudyGroupsIds = dto.IncludedStudyGroups?.Select(x => x.Id).ToList() ?? [],
            ShortTitle = dto.ShortTitle.Trim(TrimChars),
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
            IsChampionPath = dto.IsChampionPath,
            NoAgeRestrictions = dto.NoAgeRestrictions,
            MinsportSectionId = dto.MinsportSectionId
        };

    public static void SetToDraft(this WorkshopV2Dto dto, OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft model)
    {
        model.ProviderId = dto.ProviderId;
        model.WorkshopId = dto.Id == Guid.Empty ? null : dto.Id;
        model.WorkshopDraftContent = dto.ToDraftContent();
        // This is needed for search
        model.CATOTTGId = dto.Contacts.SingleOrDefault(c => c.IsDefault)?.Address?.CATOTTGId ?? 0;
    }

    public static OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft ToDraft(this WorkshopV2Dto dto)
        => new()
        {
            ProviderId = dto.ProviderId,
            WorkshopId = dto.Id == Guid.Empty ? null : dto.Id,
            CoverImageId = dto.CoverImageId,
            WorkshopDraftContent = dto.ToDraftContent(),
            Teachers = dto.Teachers?.ToDraft(),
            // This is needed for search
            CATOTTGId = dto.Contacts.SingleOrDefault(c => c.IsDefault)?.Address?.CATOTTGId ?? 0,
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
            ProviderTitle = draft.Provider?.FullTitle,
            ProviderTitleEn = draft.Provider?.FullTitleEn,
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
            LanguageOfEducationName = draft.WorkshopDraftContent?.LanguageOfEducationName ?? default,
            StudyPeriodDates = draft.WorkshopDraftContent?.ToStudyPeriodDatesDto(),
            AgeComposition = draft.WorkshopDraftContent?.AgeComposition ?? default,
            Coverage = draft.WorkshopDraftContent?.Coverage ?? default,
            WorkshopType = draft.WorkshopDraftContent?.WorkshopType ?? default,
            ParentWorkshopId = draft.WorkshopDraftContent?.ParentWorkshopId,
            Contacts = draft.WorkshopDraftContent?.Contacts?.ToDto() ?? [],
            NoAgeRestrictions = draft.WorkshopDraftContent?.NoAgeRestrictions ?? false,
            IsChampionPath = draft.WorkshopDraftContent?.IsChampionPath ?? false,

            CoverImageId = draft.CoverImageId,
            ImageIds = draft.Images?.Select(x => x.ExternalStorageId).ToList() ?? [],
            Status = draft.WorkshopDraftContent?.WorkshopStatus ?? default,
            ProviderOwnership = draft.WorkshopDraftContent?.OwnershipType ?? default,
            MinsportSectionId = draft.WorkshopDraftContent?.MinsportSectionId ?? default,
        };

    public static List<WorkshopV2Dto> ToDto(this IEnumerable<OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft> list)
        => list.MapToList(ToDto);

    public static Workshop SetToModel(this WorkshopV2Dto dto, Workshop model)
    {
        model.Id = dto.Id;
        model.Title = dto.Title?.Trim(TrimChars);
        model.ShortTitle = dto.ShortTitle?.Trim(TrimChars);
        model.MinAge = dto.MinAge ?? default;
        model.MaxAge = dto.MaxAge ?? default;
        model.DateTimeRanges = dto.DateTimeRanges?.SetToModel(model.DateTimeRanges);
        model.IsPaid = dto.IsPaid;
        model.Price = dto.Price ?? default;
        model.PayRate = dto.PayRate ?? default;
        model.FormOfLearning = dto.FormOfLearning;
        model.AvailableSeats = dto.AvailableSeats ?? default;
        model.CompetitiveSelection = dto.CompetitiveSelection;
        model.CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription;
        model.WorkshopDescriptionItems = dto.WorkshopDescriptionItems?.SetToModel(model.WorkshopDescriptionItems)
            .Concat(model.WorkshopDescriptionItems ?? [])
            .Distinct(new WorkshopDescriptionItemComparerWithoutKeys())
            .ToList();
        model.StudyPeriodStartDate = dto.StudyPeriodDates.StartDate.ToStudyPeriodDate();
        model.StudyPeriodEndDate = dto.StudyPeriodDates.EndDate.ToStudyPeriodDate();
        model.InstitutionHierarchyId = dto.InstitutionHierarchyId;
        model.DefaultTeacher = dto.DefaultTeacher?.ToModel(dto.DefaultTeacher.Id, dto.DefaultTeacher.WorkshopId);
        model.Keywords = dto.Keywords?.Count() == 0 ? null : string.Join(Constants.MappingSeparator, dto.Keywords?.Distinct() ?? []);
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
        model.IsChampionPath = dto.IsChampionPath;
        model.MinsportSectionId = dto.MinsportSectionId;

        return model;
    }

    public static WorkshopV2Dto ToV2Dto(this Workshop model)
    {
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
            SubDirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(sd => !sd.IsDeleted).Select(sd => sd.Id).ToList() ?? [],
            Keywords = model.Keywords?.Split(Constants.MappingSeparator, StringSplitOptions.None),
            Teachers = model.Teachers?.ToNotDeletedDto() ?? [],
            ProviderId = model.ProviderId,
            ProviderTitle = model.Provider?.FullTitle,
            ProviderTitleEn = model.Provider?.FullTitleEn,
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
            LanguageOfEducationName = model.LanguageOfEducation?.Name,
            IsChampionPath = model.IsChampionPath,
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
            MinsportSectionId = model.MinsportSectionId,
        };
    }

    public static List<WorkshopV2Dto> ToV2Dto(this IEnumerable<Workshop> list)
        => list.MapToList(ToV2Dto);
    public static SportsSectionUpdateRequest ToSportSectionUpdateRequest(
        this WorkshopV2Dto dto, string baseImageUrl)
    {
        var sectionId = dto.MinsportSectionId
            ?? throw new ArgumentException("SectionId is required for update.", nameof(dto));

        var defaultContact = dto.Contacts?.FirstOrDefault(c => c.IsDefault)
            ?? throw new ArgumentException("Default contact is required.", nameof(dto));

        var phones = defaultContact.Phones?
            .Select(p => new string(p.Number.Where(char.IsDigit).ToArray()))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList() ?? new();

        if (phones.Count == 0)
            throw new ArgumentException("At least one phone number is required.", nameof(dto));

        var email = defaultContact.Emails?.FirstOrDefault()?.Address
            ?? throw new ArgumentException("Default contact email is required.", nameof(dto));

        var registrationFlow = dto.EnrollmentProcedureDescription
            ?? throw new ArgumentException("EnrollmentProcedureDescription is required.", nameof(dto));

        return new SportsSectionUpdateRequest
        {
            SectionId = sectionId,

            SectionName = dto.Title,
            SectionAgeFrom = dto.MinAge ?? 0,
            SectionAgeTo = dto.MaxAge ?? 0,
            SectionIsInShlyahProject = dto.IsChampionPath,
            SectionPozashkillyaModerationStatus = ModerationStatus.ACTIVE,

            SectionAddressLocalityDictIdCode = defaultContact.Address?.CATOTTGId.ToString(),
            SectionAddressStreet = defaultContact.Address?.Street ?? string.Empty,
            SectionAddressHouse = defaultContact.Address?.BuildingNumber ?? string.Empty,

            SectionDescription = string.Join("\n", dto.WorkshopDescriptionItems.Select(x => x.Description)),
            SectionRegistrationFlow = registrationFlow,
            
            SectionPhones = phones,
            SectionEmail = email,
            SectionRegistrationFormUrl = "https://forms.example.com",
            SectionUrl = defaultContact.SocialNetworks.FirstOrDefault(s => s.Type == SocialNetworkContactType.Website)?.Url,
            SectionFacebookUrl = defaultContact.SocialNetworks.FirstOrDefault(s => s.Type == SocialNetworkContactType.Facebook)?.Url,
            SectionInstagramUrl = defaultContact.SocialNetworks.FirstOrDefault(s => s.Type == SocialNetworkContactType.Instagram)?.Url,

            SectionPracticeFormat = dto.FormOfLearning.ToSectionPracticeFormat(),
            SectionSelectionCriteria = dto.CompetitiveSelectionDescription,
            SectionPracticeCost = dto.Price ?? 0,
            SectionMaxStudentsAmount = dto.AvailableSeats == uint.MaxValue
                ? 1000
                : Math.Min((int)dto.AvailableSeats.Value, 1000),

            SectionTitlePhoto = string.IsNullOrEmpty(dto.CoverImageId) ? null
                : CombineImageUrl(baseImageUrl, dto.CoverImageId),
            SectionPhotos = dto.ImageIds?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => CombineImageUrl(baseImageUrl, id))
                .ToList() ?? new(),
            
            //TODO: We need to manage this property later, after the Ministry of Sport finishes their trainers logic.
            SectionTrainers = new List<Guid>(),
           
            SectionPracticePeriodDateFrom = $"{dto.StudyPeriodDates.StartDate.Day:D2}:{dto.StudyPeriodDates.StartDate.Month:D2}",
            SectionPracticePeriodDateTo   = $"{dto.StudyPeriodDates.EndDate.Day:D2}:{dto.StudyPeriodDates.EndDate.Month:D2}",

            SectionSchedule = dto.DateTimeRanges?
                .SelectMany(r => (r.Workdays ?? Enumerable.Empty<DaysBitMask>())
                    .SelectMany(flags => DecomposeFlags(flags)
                        .Select(day => new SectionScheduleRequest
                        {
                            SectionScheduleWeekday = Enum.Parse<Weekday>(day.ToString(), ignoreCase: true),
                            SectionScheduleTimeFrom = r.StartTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
                            SectionScheduleTimeTo   = r.EndTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture),
                        })))
                .ToList() ?? new()

        };
    }

    private static SectionPracticeFormat ToSectionPracticeFormat(this FormOfLearning formOfLearning)
        => formOfLearning switch
        {
            FormOfLearning.Online => SectionPracticeFormat.ONLINE,
            FormOfLearning.Offline => SectionPracticeFormat.OFFLINE,
            FormOfLearning.Mixed => SectionPracticeFormat.HYBRID,
            _ => throw new ArgumentOutOfRangeException(nameof(formOfLearning), formOfLearning, null)
        };

    private static IEnumerable<DaysBitMask> DecomposeFlags(DaysBitMask flags)
        => Enum.GetValues<DaysBitMask>().Where(d => d != DaysBitMask.None && flags.HasFlag(d));

    private static string CombineImageUrl(string baseUrl, string imageId)
    {
        var safeBase = (baseUrl ?? string.Empty).Trim().TrimEnd('/');
        var safeId = (imageId ?? string.Empty).Trim().TrimStart('/');
        return string.IsNullOrEmpty(safeBase) ? safeId : $"{safeBase}/{safeId}";
    }
}
