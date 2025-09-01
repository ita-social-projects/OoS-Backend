using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using System.Diagnostics.CodeAnalysis;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

// Extension methods to convert WorkshopDraft to DTO for Sports Registry
public static class WorkshopDraftToSportSectionExtensions
{
    // Maximum number of students allowed per section according to Ministry of Sport API
    private const int MinSportMaxStudentsLimit = 1000;

    // Converts WorkshopDraft into SportsSectionPostRequest DTO for sending to the Sports Registry.
    public static SportsSectionPostRequest ToSportSectionPostRequest(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft, [NotNull] string baseImageUrl)
    {
        if (string.IsNullOrWhiteSpace(baseImageUrl)) throw new ArgumentException("Base image URL is required.", nameof(baseImageUrl));
        var content = draft.WorkshopDraftContent ?? throw new ArgumentNullException(nameof(draft), "WorkshopDraftContent cannot be null");

        // Pick the default contact from draft content
        var defaultContact = content.Contacts?.FirstOrDefault(c => c.IsDefault)
              ?? throw new ArgumentException("Default contact is required.", nameof(draft)); ;

        // Extract phones, filter invalid, remove duplicates
        var phones = defaultContact.Phones?
            .Select(p => new string(p.Number.Where(char.IsDigit).ToArray()))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList() ?? new();

        if (phones.Count == 0)
            throw new ArgumentException("At least one phone number is required for the default contact.", nameof(draft));

        var registrationFlow = content.EnrollmentProcedureDescription;
        if (string.IsNullOrWhiteSpace(registrationFlow))
            throw new ArgumentException("EnrollmentProcedureDescription (SectionRegistrationFlow) is required.", nameof(draft));

        var email = defaultContact
                .Emails?
                .FirstOrDefault()?
                .Address
                ?? throw new ArgumentException("Default contact email is required.", nameof(draft));
        
        return new SportsSectionPostRequest
        {
            OrganizationCode = draft.Provider?.Edrpou ?? throw new ArgumentException("Provider is required.", nameof(draft)),
            SectionName = content.Title,

            SectionAgeFrom = content.MinAge,
            SectionAgeTo = content.MaxAge,

            SectionIsInShlyahProject = content.IsChampionPath,
            SectionPozashkillyaModerationStatus = ModerationStatus.ACTIVE,

            SectionAddressLocalityDictIdCode = draft.CATOTTGId.ToString(),
            SectionAddressStreet = defaultContact.Address?.Street ?? String.Empty,
            SectionAddressHouse = defaultContact.Address?.BuildingNumber ?? String.Empty,

            SectionDescription = string.Join("\n", content.WorkshopDescriptionItems.Select(x => x.Description)),

            SectionRegistrationFlow = registrationFlow,

            SectionPhones = phones,

            SectionEmail =email,

            SectionRegistrationFormUrl = "https://forms.example.com/football-registration", // replace with actual URL if available
            SectionUrl = defaultContact.SocialNetworks
            .FirstOrDefault(s => s.Type == SocialNetworkContactType.Website)?.Url,

            SectionFacebookUrl = defaultContact.SocialNetworks
            .FirstOrDefault(s => s.Type == SocialNetworkContactType.Facebook)?.Url,

            SectionInstagramUrl = defaultContact.SocialNetworks
            .FirstOrDefault(s => s.Type == SocialNetworkContactType.Instagram)?.Url,

            SectionPracticeFormat = content.FormOfLearning.ToSectionPracticeFormat(), // ONLINE / OFFLINE / HYBRID

            SectionSelectionCriteria = content.CompetitiveSelectionDescription,

            SectionPracticeCost = content.Price,

            SectionMaxStudentsAmount = content.AvailableSeats == uint.MaxValue
            ? MinSportMaxStudentsLimit : Math.Min((int)content.AvailableSeats, MinSportMaxStudentsLimit),

            SectionTitlePhoto = string.IsNullOrEmpty(draft.CoverImageId)
            ? null
            : CombineImageUrl(baseImageUrl, draft.CoverImageId),

            SectionPhotos = MapSectionPhotos(draft.Images, baseImageUrl),
            SectionTrainers = [], // TODO: make mapping when teachers will be added to the draft

            SectionPracticePeriodDateFrom = content.StudyPeriodStartDate.ToString(@"dd\:MM", CultureInfo.InvariantCulture),
            SectionPracticePeriodDateTo = content.StudyPeriodEndDate.ToString(@"dd\:MM", CultureInfo.InvariantCulture),

            SectionSchedule = content.DateTimeRanges?
            .SelectMany(r => (r.Workdays ?? Enumerable.Empty<DaysBitMask>())
            .SelectMany(flags => DecomposeFlags(flags)
            .Select(day => new SectionScheduleRequest
            {
                SectionScheduleWeekday = Enum.Parse<Weekday>(day.ToString(), ignoreCase: true),
                SectionScheduleTimeFrom = r.StartTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture),
                SectionScheduleTimeTo = r.EndTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture),
            }))).ToList() ?? new(),
        };
    }

   
    // Maps FormOfLearning enum to external SectionPracticeFormat enum.
    private static SectionPracticeFormat ToSectionPracticeFormat(this FormOfLearning formOfLearning)
    {
        return formOfLearning switch
        {
            FormOfLearning.Online => SectionPracticeFormat.ONLINE,
            FormOfLearning.Offline => SectionPracticeFormat.OFFLINE,
            FormOfLearning.Mixed => SectionPracticeFormat.HYBRID,
            _ => throw new ArgumentOutOfRangeException(nameof(formOfLearning), formOfLearning, "Unsupported learning format. Must be Online, Offline or Mixed."),
        };
    }


    // Decomposes a DaysBitMask flag into individual days.
    private static IEnumerable<DaysBitMask> DecomposeFlags(DaysBitMask flags)
    {
        return Enum.GetValues<DaysBitMask>()
            .Where(d => d != DaysBitMask.None && flags.HasFlag(d));
    }

    // Combines a base URL and an image ID into a single valid URL, handling slashes.
    private static string CombineImageUrl(string baseUrl, string imageId)
    {
        var safeBase = (baseUrl ?? string.Empty).Trim().TrimEnd('/');
        var safeId = (imageId ?? string.Empty).Trim().TrimStart('/');

        if (safeBase.Length == 0) return safeId;   // "/id" no return 
        if (safeId.Length == 0) return safeBase; // "base/" no return 
        return $"{safeBase}/{safeId}";
    }

    // Maps a collection of images to their public URLs using the base URL.
    private static List<string> MapSectionPhotos<T>(IEnumerable<Image<T>> images, string baseUrl)
    {
        if (images == null) return new();

        return images
            .Where(img => !string.IsNullOrWhiteSpace(img.ExternalStorageId))
            .Select(img => CombineImageUrl(baseUrl, img.ExternalStorageId))
            .Distinct()
            .ToList();
    }
}
