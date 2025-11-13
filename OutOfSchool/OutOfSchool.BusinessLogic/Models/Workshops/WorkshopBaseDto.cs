using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopBaseDto : IValidatableObject, IHasContactsDto<Workshop>
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength, ErrorMessage = "Title field must contain from 3 to 250 characters.")]
    [MaxLength(Constants.MaxWorkshopTitleLength, ErrorMessage = "Title field must contain from 3 to 250 characters.")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Title field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength, ErrorMessage = "This field must contain from 1 to 60 characters.")]
    [MaxLength(Constants.MaxWorkshopShortTitleLength, ErrorMessage = "This field must contain from 1 to 60 characters.")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "This field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string ShortTitle { get; set; } = string.Empty;

    public bool NoAgeRestrictions { get; set; } = false;

    [RequiredIf("NoAgeRestrictions", false, ErrorMessage = "Min age is required when there are age restrictions")]
    [Range(0, 120, ErrorMessage = "Min age should be between 0 and 120")]
    public int? MinAge { get; set; }

    [RequiredIf("NoAgeRestrictions", false, ErrorMessage = "Max age is required when there are age restrictions")]
    [Range(0, 120, ErrorMessage = "Max age should be between 0 and 120")]
    public int? MaxAge { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one DateTime range is required")]
    public List<DateTimeRangeDto> DateTimeRanges { get; set; }

    public bool IsPaid { get; set; } = false;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 0 to 100 000")]
    public decimal? Price { get; set; } = default;

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType? PayRate { get; set; } = PayRateType.None;

    [Required(ErrorMessage = "Form of learning is required")]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning FormOfLearning { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [Required(ErrorMessage = "Study period dates is required")]
    public StudyPeriodDatesDto StudyPeriodDates { get; set; }

    [Required(ErrorMessage = "Available seats are required")]
    public uint? AvailableSeats { get; set; } = uint.MaxValue;

    public bool CompetitiveSelection { get; set; }

    [MinLength(Constants.MinCompetitiveSelectionDescriptionLength)]
    [MaxLength(Constants.MaxCompetitiveSelectionDescriptionLength)]
    [RequiredIf(nameof(CompetitiveSelection), true, ErrorMessage = "Competitive selection description is required")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Competitive selection description must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string CompetitiveSelectionDescription { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description item is required")]
    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public string InstitutionHierarchy { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public TeacherDTO DefaultTeacher { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> DirectionIds { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> SubDirectionIds { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [MaxLength(Constants.MaxCountOfKeywordsForWorkshop)]
    public IEnumerable<string> Keywords { get; set; } = default;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<TeacherDTO> Teachers { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    public string ProviderTitle { get; set; } = string.Empty;

    public string ProviderTitleEn { get; set; } = string.Empty;

    public ProviderLicenseStatus ProviderLicenseStatus { get; set; } = ProviderLicenseStatus.NotProvided;

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool IsSelfFinanced { get; set; } = false;

    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    public bool IsInclusive { get; set; } = false;

    [Required]
    [MinLength(Constants.MinLengthOfEnrollmentProcedureDescriptionForWorkshop)]
    [MaxLength(Constants.MaxLengthOfEnrollmentProcedureDescriptionForWorkshop)]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "EnrollmentProcedureDescription field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; } = default;

    public bool IsChampionPath { get; set; } = false;

    [MinLength(Constants.MinPreferentialTermsOfParticipationLength)]
    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "PreferentialTermsOfParticipation is required")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "PreferentialTermsOfParticipation field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string PreferentialTermsOfParticipation { get; set; }

    [Required]
    [EnumDataType(typeof(EducationalShift), ErrorMessage = Constants.EnumErrorMessage)]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;

    [Required(ErrorMessage = "Language of education is required")]
    [Range(1, long.MaxValue, ErrorMessage = "LanguageOfEducationId must be a positive number")]
    public long LanguageOfEducationId { get; set; }

    [Required(ErrorMessage = "Type of age composition is required")]
    [EnumDataType(typeof(AgeComposition), ErrorMessage = Constants.EnumErrorMessage)]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;

    [Required]
    [EnumDataType(typeof(WorkshopType), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopType WorkshopType { get; set; } = WorkshopType.Workshop;

    public Guid? DefaultTeacherId { get; set; }

    public Guid? ParentWorkshopId { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public WorkshopBaseDto ParentWorkshop { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public virtual ICollection<WorkshopBaseDto> IncludedStudyGroups { get; set; } // Navigation property to included study groups

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one contact is required")]
    public List<ContactsDto> Contacts { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AvailableSeats != uint.MaxValue && (AvailableSeats < 1 || AvailableSeats > 100000))
        {
            yield return new ValidationResult("AvailableSeats field should be in the range from 1 to 100000.", [nameof(AvailableSeats)]);
        }

        foreach (var dateTimeRange in DateTimeRanges)
        {
            if (dateTimeRange.StartTime >= dateTimeRange.EndTime)
            {
                yield return new ValidationResult(
                     "The end date cannot be equal to or earlier than the start date");
            }

            if (dateTimeRange.Workdays.IsNullOrEmpty() || dateTimeRange.Workdays.Any(workday => workday == DaysBitMask.None))
            {
                yield return new ValidationResult(
                    "Workdays are required");
            }

            var daysHs = new HashSet<DaysBitMask>();
            if (!dateTimeRange.Workdays.All(daysHs.Add))
            {
                yield return new ValidationResult(
                    "Workdays contain duplications");
            }
        }

        if (!NoAgeRestrictions && MinAge >= MaxAge)
        {
            yield return new ValidationResult("Min age should be less than Max age", [nameof(MinAge), nameof(MaxAge)]);
        }

        // validate Price and PayRate when IsPaid is true
        if (IsPaid)
        {
            if (PayRate == null || PayRate == PayRateType.None)
            {
                yield return new ValidationResult("Pay rate must be specified when the workshop is paid.", [nameof(PayRate)]);
            }

            if (!Price.HasValue || Price < 0.01M)
            {
                yield return new ValidationResult("Price must be specified and must be in the range from 0.01 to 100000.00 when the workshop is paid.", [nameof(Price)]);
            }
        }

        if (!Keywords.IsNullOrEmpty())
        {
            var keywordsList = Keywords.ToList();
            var cleanedKeywords = new List<string>(keywordsList.Count);
            int totalLength = 0;

            // Check for null/whitespace, trim, check single length, and calculate total length
            foreach (var keyword in keywordsList)
            {
                // Check 1: Empty or whitespace
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    yield return new ValidationResult(
                        "Keyword cannot be empty or whitespace.",
                        [nameof(Keywords)]);
                }

                string trimmedKeyword = keyword?.Trim();
                cleanedKeywords.Add(trimmedKeyword);
                totalLength += trimmedKeyword.Length;

                // Check 2: Single keyword max length
                if (trimmedKeyword.Length > Constants.MaxLengthOfOneKeyword)
                {
                    yield return new ValidationResult(
                        $"Keyword must be no longer than {Constants.MaxLengthOfOneKeyword} characters.",
                        [nameof(Keywords)]);
                }
            }

            // Check 3: Total length of all keywords
            if (totalLength > Constants.MaxKeywordsLength)
            {
                yield return new ValidationResult(
                    $"The length of all keywords must not exceed {Constants.MaxKeywordsLength} characters.",
                    [nameof(Keywords)]);
            }

            // Check 4: Duplicates (case-insensitive)
            HashSet<string> keywordsSet = new(StringComparer.OrdinalIgnoreCase);

            if (!cleanedKeywords.All(keywordsSet.Add))
            {
                yield return new ValidationResult("Keywords list contains duplicates.", [nameof(Keywords)]);
            }
        }
    }
}