using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.BusinessLogic.Models.Exported.Workshops;

public class WorkshopInfoDto : WorkshopInfoBaseDto, IExternalRatingInfo
{
    public Guid ProviderId { get; set; }

    public Guid? ParentWorkshopId { get; set; }

    public int TakenSeats { get; set; } = 0;

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(1)]
    [MaxLength(60)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Website { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Facebook { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.MaxUnifiedUrlLength)]
    public string Instagram { get; set; } = string.Empty;

    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    [Required]
    public List<DateTimeRangeDto> DateTimeRanges { get; set; }

    public bool IsPaid { get; set; } = false;

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal? Price { get; set; } = default;

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType? PayRate { get; set; } = PayRateType.Classes;

    [Required(ErrorMessage = "Form of learning is required")]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning FormOfLearning { get; set; }

    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public bool CompetitiveSelection { get; set; }

    [MaxLength(500)]
    public string CompetitiveSelectionDescription { get; set; }

    [CollectionNotEmpty(ErrorMessage = "At least one description is required")]
    public IEnumerable<WorkshopDescriptionItemInfo> WorkshopDescriptionItems { get; set; }

    public bool WithDisabilityOptions { get; set; } = default;

    [MaxLength(200)]
    public string DisabilityOptionsDesc { get; set; } = string.Empty;

    public string Institution { get; set; }

    public string InstitutionHierarchy { get; set; }

    public TeacherInfoDto DefaultTeacher { get; set; }

    public List<long> DirectionIds { get; set; }
    
    public Guid SubDirectionId { get; set; }

    public IEnumerable<string> Keywords { get; set; } = default;

    [Required]
    public AddressInfoDto Address { get; set; }

    public List<TeacherInfoDto> Teachers { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool ShortStay { get; set; } = false;

    public bool IsSelfFinanced { get; set; } = false;

    public bool IsSpecial { get; set; } = false;

    public bool IsInclusive { get; set; } = false;
    
    [Required(ErrorMessage = "Workshop type is required")]
    [EnumDataType(typeof(WorkshopType), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopType WorkshopType { get; set; }
    
    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    // TODO: Need to implement
    public string LanguageOfEducation { get; set; } = "українська";

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    public IList<string> ImageIds { get; set; }

    public List<string> Tags { get; set; }
    
    public Guid? DefaultTeacherId { get; set; }
    
    [MaxLength(500)]
    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; } = default;

    [MaxLength(500)]
    public string PreferentialTermsOfParticipation { get; set; }

    [Required]
    [EnumDataType(typeof(EducationalShift), ErrorMessage = Constants.EnumErrorMessage)]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;

    [Required(ErrorMessage = "Type of age composition is required")]
    [EnumDataType(typeof(AgeComposition), ErrorMessage = Constants.EnumErrorMessage)]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;
}