using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Validators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft;

[DateOrder]
public class WorkshopDraftBaseDto
{
    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    [Required]
    public Guid InstitutionId { get; set; }

    [Required]
    [EnumDataType(typeof(EducationalShift), ErrorMessage = Constants.EnumErrorMessage)]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;

    [Required]
    public bool ShortStay { get; set; }

    [Required]
    public uint LanguageOfEducationId { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one value in the DateTime range is required.")]
    [CollectionValidation(typeof(DateTimeRangeValidator), nameof(DateTimeRangeValidator.Validate))]
    public List<DateTimeRangeDraftDto> DateTimeRanges { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description is required.")]
    public List<WorkshopDescriptionItemDraftDto> WorkshopDescriptionItems { get; set; }

    [Required]
    public bool IsSelfFinanced { get; set; }

    [Required(ErrorMessage = "Type of age composition is required")]
    [EnumDataType(typeof(AgeComposition), ErrorMessage = Constants.EnumErrorMessage)]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;

    [Required]
    public bool CompetitiveSelection { get; set; }

    [Required]
    public DateOnly ActiveFrom { get; set; }

    [Required]
    public DateOnly ActiveTo { get; set; }

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; }

    [Required]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitle { get; set; }

    [Required]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitleEn { get; set; }

    [CollectionNotEmpty(ErrorMessage = "At least one Direction ID is required.")]
    public List<long> DirectionIds { get; set; }

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;

    [Required]
    public bool IsPaid { get; set; }

    [Required]
    public bool IsSpecial { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    [Required]
    public bool IsInclusive { get; set; }

    [Required]
    public string Keywords { get; set; }

    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType OwnershipType { get; set; }

    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus ProviderLicenseStatus { get; set; }

    public bool WithDisabilityOptions { get; set; }

    [MaxLength(Constants.DisabilityOptionsLength)]
    public string DisabilityOptionsDesc { get; set; }

    public uint AvailableSeats { get; set; }

    public List<Guid> IncludedStudyGroupsIds { get; set; }

    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; }

    [MaxLength(Constants.WorkshopDraftDescriptionMaxLength)]
    public string CompetitiveSelectionDescription { get; set; }

    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning FormOfLearning { get; set; }

    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus WorkshopStatus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal Price { get; set; }

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType PayRate { get; set; }

    public bool AreThereBenefits { get; set; }

    [MaxLength(Constants.WorkshopDraftDescriptionMaxLength)]
    public string PreferentialTermsOfParticipation { get; set; }

    public Guid InstitutionHierarchyId { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.MaxPhoneNumberLengthWithPlusSign)]
    public string Phone { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(Constants.MaxEmailAddressLength)]
    public string Email { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Website { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Facebook { get; set; }

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Instagram { get; set; }
}
