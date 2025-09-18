using System.Diagnostics.CodeAnalysis;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

public static class WorkshopDraftToSportSectionExtensions
{
    private const int MinSportMaxStudentsLimit = 1000;

    public static SportsSectionPostRequest ToSportSectionPostRequest(
        this WorkshopDraft draft, [NotNull] string baseImageUrl)
    {
        return new SportsSectionPostRequest
        {
            OrganizationCode = draft.Provider?.Edrpou
                ?? throw new ArgumentException("Provider is required.", nameof(draft)),
        }.WithBaseData(draft, baseImageUrl);
    }

    public static SportsSectionUpdateRequest ToSportSectionUpdateRequest(
        this WorkshopDraft draft, [NotNull] string baseImageUrl)
    {
        var sectionId = draft.WorkshopDraftContent?.MinsportSectionId
            ?? throw new ArgumentException("SectionId is required for update.", nameof(draft));

        return new SportsSectionUpdateRequest
        {
            SectionId = sectionId
        }.WithBaseData(draft, baseImageUrl);
    }

    /// <summary>
    /// Universal mapping for every inherited dto from SportsSectionBaseDto.
    /// </summary>
    private static T WithBaseData<T>(this T target, WorkshopDraft draft, string baseImageUrl)
        where T : SportsSectionBaseDto
    {
        var content = draft.WorkshopDraftContent
            ?? throw new ArgumentNullException(nameof(draft), "WorkshopDraftContent cannot be null");

        var defaultContact = content.Contacts?.FirstOrDefault(c => c.IsDefault)
            ?? throw new ArgumentException("Default contact is required.", nameof(draft));

        var phones = defaultContact.Phones?
            .Select(p => new string(p.Number.Where(char.IsDigit).ToArray()))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList() ?? new();

        if (phones.Count == 0)
            throw new ArgumentException("At least one phone number is required.", nameof(draft));

        var email = defaultContact.Emails?.FirstOrDefault()?.Address
            ?? throw new ArgumentException("Default contact email is required.", nameof(draft));

        var registrationFlow = content.EnrollmentProcedureDescription
            ?? throw new ArgumentException("EnrollmentProcedureDescription is required.", nameof(draft));

        target.SectionName = content.Title;
        target.SectionAgeFrom = content.MinAge;
        target.SectionAgeTo = content.MaxAge;
        target.SectionIsInShlyahProject = content.IsChampionPath;
        target.SectionPozashkillyaModerationStatus = ModerationStatus.ACTIVE;

        target.SectionAddressLocalityDictIdCode = draft.CATOTTGId.ToString();
        target.SectionAddressStreet = defaultContact.Address?.Street ?? string.Empty;
        target.SectionAddressHouse = defaultContact.Address?.BuildingNumber ?? string.Empty;

        target.SectionDescription = string.Join("\n", content.WorkshopDescriptionItems.Select(x => x.Description));
        target.SectionRegistrationFlow = registrationFlow;

        target.SectionPhones = phones;
        target.SectionEmail = email;
        target.SectionRegistrationFormUrl = "https://forms.example.com/football-registration";
        target.SectionUrl = defaultContact.SocialNetworks.FirstOrDefault(s => s.Type == SocialNetworkContactType.Website)?.Url;
        target.SectionFacebookUrl = defaultContact.SocialNetworks.FirstOrDefault(s => s.Type == SocialNetworkContactType.Facebook)?.Url;
        target.SectionInstagramUrl = defaultContact.SocialNetworks.FirstOrDefault(s => s.Type == SocialNetworkContactType.Instagram)?.Url;

        target.SectionPracticeFormat = content.FormOfLearning.ToSectionPracticeFormat();
        target.SectionSelectionCriteria = content.CompetitiveSelectionDescription;
        target.SectionPracticeCost = content.Price;
        target.SectionMaxStudentsAmount = content.AvailableSeats == uint.MaxValue
            ? MinSportMaxStudentsLimit
            : Math.Min((int)content.AvailableSeats, MinSportMaxStudentsLimit);

        target.SectionTitlePhoto = string.IsNullOrEmpty(draft.CoverImageId) ? null
            : CombineImageUrl(baseImageUrl, draft.CoverImageId);
        target.SectionPhotos = MapSectionPhotos(draft.Images, baseImageUrl);
        target.SectionTrainers = new List<Guid>();

        target.SectionPracticePeriodDateFrom = content.StudyPeriodStartDate.ToString(@"dd\:MM", CultureInfo.InvariantCulture);
        target.SectionPracticePeriodDateTo = content.StudyPeriodEndDate.ToString(@"dd\:MM", CultureInfo.InvariantCulture);

        target.SectionSchedule = content.DateTimeRanges?
            .SelectMany(r => (r.Workdays ?? Enumerable.Empty<DaysBitMask>())
            .SelectMany(flags => DecomposeFlags(flags)
            .Select(day => new SectionScheduleRequest
            {
                SectionScheduleWeekday = Enum.Parse<Weekday>(day.ToString(), ignoreCase: true),
                SectionScheduleTimeFrom = r.StartTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture),
                SectionScheduleTimeTo = r.EndTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture),
            })))
            .ToList() ?? new();

        return target;
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

    private static List<string> MapSectionPhotos<T>(IEnumerable<Image<T>> images, string baseUrl)
        => images?
            .Where(img => !string.IsNullOrWhiteSpace(img.ExternalStorageId))
            .Select(img => CombineImageUrl(baseUrl, img.ExternalStorageId))
            .Distinct()
            .ToList() ?? new();
}
