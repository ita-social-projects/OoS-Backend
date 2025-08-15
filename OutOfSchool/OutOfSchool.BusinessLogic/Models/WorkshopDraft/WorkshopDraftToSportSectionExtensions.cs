using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using System.Diagnostics.CodeAnalysis;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;
public static class WorkshopDraftToSportSectionExtensions
{
    public static SportsSectionPostRequest ToSportSectionPostRequest(this OutOfSchool.Services.Models.WorkshopDrafts.WorkshopDraft draft, [NotNull] string baseImageUrl) // check not null
    {
        var content = draft.WorkshopDraftContent ?? throw new ArgumentNullException(nameof(draft.WorkshopDraftContent));; 
        var defaultContact = content.Contacts?.FirstOrDefault(c => c.IsDefault);
        return new SportsSectionPostRequest
        {
            OrganizationCode = draft.Provider.Edrpou,
            SectionName = content.Title,

            SectionSportKindDictIdCode = 55,

            SectionAgeFrom = content.MinAge,
            SectionAgeTo = content.MaxAge,

            SectionIsInShlyahProject = content.IsChampionPath,

            SectionAddressLocalityDictIdCode = "UA26100070020012343", /*defaultContact?.Address?.CATOTTGId.ToString() , // is it OK?*/
            SectionAddressStreet = defaultContact?.Address?.Street,
            SectionAddressHouse = defaultContact?.Address?.BuildingNumber,

            SectionDescription = string.Join("\n", content.WorkshopDescriptionItems.Select(x => x.Description)),

            SectionRegistrationFlow = content.EnrollmentProcedureDescription,

            SectionPhones = defaultContact?.Phones?
                .Select(p => new string(p.Number.Where(char.IsDigit).ToArray()))
                .Distinct().ToList() ?? new(),

            SectionEmail = content.Contacts?
                .FirstOrDefault(c => c.IsDefault)?
                .Emails?
                .FirstOrDefault()?
                .Address,

            SectionRegistrationFormUrl = "https://forms.example.com/football-registration",
            SectionUrl = defaultContact?.SocialNetworks
            .FirstOrDefault(s => s.Type == SocialNetworkContactType.Website)?.Url,

            SectionFacebookUrl = defaultContact?.SocialNetworks
            .FirstOrDefault(s => s.Type == SocialNetworkContactType.Facebook)?.Url,

            SectionInstagramUrl = defaultContact?.SocialNetworks
            .FirstOrDefault(s => s.Type == SocialNetworkContactType.Instagram)?.Url,

            SectionPracticeFormat = content.FormOfLearning.ToSectionPracticeFormat(), // ONLINE / OFFLINE / HYBRID

            SectionSelectionCriteria = content.CompetitiveSelectionDescription,

            SectionPracticeCost = content.Price,

            SectionMaxStudentsAmount = content.AvailableSeats == uint.MaxValue
            ? 1000 : (int)content.AvailableSeats,

            SectionTitlePhoto = string.IsNullOrEmpty(draft.CoverImageId) 
            ? null
            : CombineImageUrl(baseImageUrl, draft.CoverImageId),

            /*SectionPhotos = draft.Images?
            .Where(img => !string.IsNullOrWhiteSpace(img.ExternalStorageId))
            .Select(img => CombineImageUrl(baseImageUrl, img.ExternalStorageId))
            .ToList() ?? new(),
            */
            SectionPhotos = [
                "https://images.unsplash.com/photo-1529778873920-4da4926a72c2?fm=jpg&q=60&w=3000&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Mnx8Y3V0ZSUyMGNhdHxlbnwwfHwwfHx8MA%3D%3D",
                "https://www.gstatic.com/images/branding/googlelogo/svg/googlelogo2_clr_160x56px.svg",
                "https://www.gstatic.com/images/branding/googlelogo/svg/googlelogo3_clr_160x56px.svg"
            ],
            SectionTrainers = [], // TODO: make mapping when teachers will be added to the draft

            SectionPracticePeriodDateFrom = content.StudyPeriodStartDate.ToString("dd':'MM", CultureInfo.InvariantCulture),
            SectionPracticePeriodDateTo   = content.StudyPeriodEndDate.ToString("dd':'MM", CultureInfo.InvariantCulture),

            //SectionSchedule = content.DateTimeRanges?
            //.SelectMany(r => r.Workdays.Select(day => new SectionScheduleRequest
            //{
            //    SectionScheduleWeekday = Enum.Parse<Weekday>(day.ToString(), ignoreCase: true),
            //    SectionScheduleTimeFrom = r.StartTime.ToString("HH:mm:ss"),
            //    SectionScheduleTimeTo = r.EndTime.ToString("HH:mm:ss"),
            //}))
            //.ToList() ?? new(),
            SectionSchedule = content.DateTimeRanges?.SelectMany(r =>
                 r.Workdays
                .SelectMany(flags => DecomposeFlags(flags)
                .Select(day => new SectionScheduleRequest
                {
                    SectionScheduleWeekday = Enum.Parse<Weekday>(day.ToString(), ignoreCase: true),
                    SectionScheduleTimeFrom = r.StartTime.ToString("HH:mm:ss"),
                    SectionScheduleTimeTo = r.EndTime.ToString("HH:mm:ss"),
                }))).ToList() ?? new(),
        };
    }
    public static SectionPracticeFormat ToSectionPracticeFormat(this FormOfLearning formOfLearning)
    {
        return formOfLearning switch
        {
            FormOfLearning.Online => SectionPracticeFormat.ONLINE,
            FormOfLearning.Offline => SectionPracticeFormat.OFFLINE,
            FormOfLearning.Mixed => SectionPracticeFormat.HYBRID,
            _ => throw new ArgumentOutOfRangeException(nameof(formOfLearning), formOfLearning, "Unsupported learning format. Must be Online, Offline or Mixed."),
        };
    }
    public static IEnumerable<DaysBitMask> DecomposeFlags(DaysBitMask flags)
    {
        return Enum.GetValues<DaysBitMask>()
            .Where(d => d != DaysBitMask.None && flags.HasFlag(d));
    }
    private static string CombineImageUrl(string baseUrl, string imageId)
    {
        return $"{baseUrl.TrimEnd('/')}/{imageId.TrimStart('/')}";  //$"{baseImageUrl}{draft.CoverImageId}"
    }
}
