using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;
using OutOfSchool.Common;
using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Validators;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SportsSectionBaseDto
{
    [Required(ErrorMessage = "sectionName is required.")]
    public string SectionName { get; set; } = null!;

    [Required(ErrorMessage = "sectionSportKindDictIdCode is required.")]
    [Range(typeof(long), "1", "9223372036854775807", ErrorMessage = "sectionSportKindDictIdCode must be a non-negative integer.")]
    public long SectionSportKindDictIdCode { get; set; }

    [Required(ErrorMessage = "sectionAgeFrom is required.")]
    [Range(0, RegistryConstants.MaxAge, ErrorMessage = "sectionAgeFrom must be a non-negative Integer less than or equal to 120.")]
    public int SectionAgeFrom { get; set; }

    [Required(ErrorMessage = "sectionAgeTo is required.")]
    [Range(0, RegistryConstants.MaxAge, ErrorMessage = "sectionAgeTo must be a non-negative Integer less than or equal to 120.")]
    public int SectionAgeTo { get; set; }
    public bool SectionIsInShlyahProject { get; set; }

    [Required(ErrorMessage = "sectionAddressRegionDictIdCode is required.")]
    public string SectionAddressLocalityDictIdCode { get; set; } = null!;

    [Required(ErrorMessage = "sectionAddressStreet is required.")]
    public string SectionAddressStreet { get; set; } = null!;

    [Required(ErrorMessage = "sectionAddressHouse is required.")]
    public string SectionAddressHouse { get; set; } = null!;

    [Required(ErrorMessage = "sectionDescription is required.")]
    public string SectionDescription { get; set; } = null!;

    [Required(ErrorMessage = "sectionRegistrationFlow is required.")]
    public string SectionRegistrationFlow { get; set; } = null!;

    [Required(ErrorMessage = "sectionPhone is required.")]
    [MinLength(1, ErrorMessage = "At least one section phone must be provided.")]
    [PhoneListValidation]
    public List<string> SectionPhones { get; set; } = new();

    [Required(ErrorMessage = "sectionEmail is required.")]
    [RegularExpression(Constants.EmailRegexViewModel, ErrorMessage = "Invalid email format")]
    public string SectionEmail { get; set; } = null!;

    [Required(ErrorMessage = "sectionRegistrationFormUrl is required.")]
    [Url(ErrorMessage = "sectionRegistrationFormUrl must be a valid URL.")]
    public string SectionRegistrationFormUrl { get; set; } = null!;

    [Url(ErrorMessage = "Invalid URL format. SectionUrl must be a valid URL.")]
    public string? SectionUrl { get; set; }

    [RegularExpression(@"^(https?://)?(www\.)?facebook\.com/.*$", ErrorMessage = "sectionFacebookUrl must be a valid Facebook URL.")]
    public string? SectionFacebookUrl { get; set; }

    [RegularExpression(@"^https?://(?:[\w-]+\.)*instagram\.com(/[^\s]*)?$", ErrorMessage = "Invalid Instagram URL format. SectionInstagramUrl must be a valid Instagram URL.")]
    public string? SectionInstagramUrl { get; set; }

    [Required(ErrorMessage = "sectionPracticeFormat is required.")]
    public SectionPracticeFormat SectionPracticeFormat { get; set; } // OFFLINE / ONLINE / HYBRID
    public string? SectionSelectionCriteria { get; set; }

    [Required(ErrorMessage = "sectionPracticeCost is required.")]
    [Range(0, 100000.0, ErrorMessage = "sectionPracticeCost cannot be negative or exceed 100000.")]
    public decimal SectionPracticeCost { get; set; }

    [Required(ErrorMessage = "sectionMaxStudentsAmount is required.")]
    [Range(1, 1000, ErrorMessage = "sectionMaxStudentsAmount must be between 1 and 1000.")]
    public int SectionMaxStudentsAmount { get; set; }

    public string? SectionTitlePhoto { get; set; }
    public List<string> SectionPhotos { get; set; } = new();

    public List<Guid> SectionTrainers { get; set; } = new();

    [Required(ErrorMessage = "sectionPracticePeriodDateFrom is required.")]
    [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Date must be in DD:MM format.")]
    public string SectionPracticePeriodDateFrom { get; set; } = null!;

    [Required(ErrorMessage = "sectionPracticePeriodDateTo is required.")]
    [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Date must be in DD:MM format.")]
    public string SectionPracticePeriodDateTo { get; set; } = null!;


    [Required(ErrorMessage = "sectionSchedule is required.")]
    [MinLength(1, ErrorMessage = "At least one section schedule is required.")]
    public List<SectionScheduleRequest> SectionSchedule { get; set; } = new();

    [Required(ErrorMessage = "sectionPozashkillyaModerationStatus is required.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ModerationStatus SectionPozashkillyaModerationStatus { get; set; } = ModerationStatus.DRAFT;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // section age validation
        if (SectionAgeFrom < 0 || SectionAgeTo < 0)
        {
            yield return new ValidationResult(
                "Age values cannot be negative.",
                new[] { nameof(SectionAgeFrom) });
        }

        if (SectionAgeFrom > SectionAgeTo)
        {
            yield return new ValidationResult(
                "SectionAgeFrom cannot be greater than SectionAgeTo.",
                new[] { nameof(SectionAgeFrom), nameof(SectionAgeTo) });
        }

        // validate section photo URLs
        if (SectionPhotos != null && SectionPhotos.Any())
        {
            var invalidUrls = SectionPhotos
                .Where(url => !(Uri.TryCreate(url, UriKind.Absolute, out var uri)
                                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)));

            if (invalidUrls.Any())
            {
                yield return new ValidationResult(
                    $"Invalid photo URLs: {string.Join(", ", invalidUrls)}",
                    new[] { nameof(SectionPhotos) });
            }
        }

        // validate section practice period dates
        var culture = CultureInfo.InvariantCulture;

        bool isValidStart = DateTime.TryParseExact(
            SectionPracticePeriodDateFrom,
            "dd:MM",
            culture,
            DateTimeStyles.None,
            out var dateFrom);

        bool isValidEnd = DateTime.TryParseExact(
            SectionPracticePeriodDateTo,
            "dd:MM",
            culture,
            DateTimeStyles.None,
            out var dateTo);

        if (isValidStart && isValidEnd)
        {
            if (dateFrom > dateTo)
            {
                yield return new ValidationResult(
                    "Practice period start date cannot be after end date.",
                    new[] { nameof(SectionPracticePeriodDateFrom), nameof(SectionPracticePeriodDateTo) });
            }
        }
    }
}