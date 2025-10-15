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
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    [MustContain(RequiredCharacterType.AnyLetter)]
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
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
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

    [MinLength(3)]
    [MaxLength(500)]
    [RequiredIf(nameof(CompetitiveSelection), true, ErrorMessage = "CompetitiveSelectionDescription field is required")]
    [MustContain(RequiredCharacterType.AnyLetter)]
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
    public IEnumerable<string> Keywords { get; set; } = default;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<TeacherDTO> Teachers { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitle { get; set; } = string.Empty;

    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitleEn { get; set; } = string.Empty;

    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus ProviderLicenseStatus { get; set; } = ProviderLicenseStatus.NotProvided;

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool IsSelfFinanced { get; set; } = false;

    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    public bool IsInclusive { get; set; } = false;

    [MinLength(3)]
    [MaxLength(2000)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; } = default;

    public bool IsChampionPath { get; set; } = false;

    [MinLength(3)]
    [MaxLength(500)]
    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "PreferentialTermsOfParticipation is required")]
    [MustContain(RequiredCharacterType.AnyLetter)]
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
    public List<ContactsDto> Contacts { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // TODO: Validate DateTimeRanges are not empty when frontend is ready
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

        if (NoAgeRestrictions)
        {
            MinAge = 0;
            MaxAge = 120;
        }
        else if (MinAge.HasValue && MaxAge.HasValue && MinAge >= MaxAge)
        {
            yield return new ValidationResult("Min age should be less than Max age", new[] { nameof(MinAge), nameof(MaxAge) });
        }

        // validate Price and PayRate when IsPaid is true
        if (IsPaid)
        {
            if (PayRate == null || PayRate == PayRateType.None)
            {
                yield return new ValidationResult("Pay rate must be specified when the workshop is paid.", new[] { nameof(PayRate) });
            }

            if (!Price.HasValue)
            {
                yield return new ValidationResult("Price must be specified when the workshop is paid.", new[] { nameof(Price) });
            }
            else
            {
                if (Price < 1.00m)
                {
                    yield return new ValidationResult("Price must be at least 1.00 if the workshop is paid.", new[] { nameof(Price) });
                }

                if (Price > 100000.00m)
                {
                    yield return new ValidationResult("Price must be less than or equal to 100000.00.", new[] { nameof(Price) });
                }
            }
        }
        else 
        { 
            PayRate = PayRateType.None;
        }

        if (!Keywords.IsNullOrEmpty())
        {
            var keywordsList = Keywords.ToList();

            if (keywordsList.Count > 5)
            {
                yield return new ValidationResult("Keywords list should contain no more than 5 words", new[] { nameof(Keywords) });
            }

            if (keywordsList.Any(string.IsNullOrWhiteSpace))
            {
                yield return new ValidationResult(
                    "Keyword cannot be empty or whitespace.",
                    new[] { nameof(Keywords) });
            }

            if (keywordsList.Any(k => !string.IsNullOrWhiteSpace(k) && k.Length > 60))
            {
                yield return new ValidationResult(
                    "Keyword must be no longer than 60 characters.",
                    new[] { nameof(Keywords) });
            }

            var cleanedKeyWordsList = keywordsList
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim());
            
            HashSet<string> keywordsSet = new(StringComparer.OrdinalIgnoreCase);
            
            if (!cleanedKeyWordsList.All(keywordsSet.Add))
            {
                yield return new ValidationResult("Keywords list contains duplicates.", new[] { nameof(Keywords) });
            }
        }

        if (AvailableSeats != uint.MaxValue && (AvailableSeats < 1 || AvailableSeats > 100000))
        {
            yield return new ValidationResult("AvailableSeats field should be in the range from 1 to 100000.", new[] { nameof(AvailableSeats) });
        }
    }
}