using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

// TODO:
// - Add educational disciplines (many ED to 1 workshop)
// - Add language
public class Workshop : BusinessEntity, IImageDependentEntity<Workshop>, IHasEntityImages<Workshop>
{
    #region Required fields

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    [Required(ErrorMessage = "Available seats are required")]
    public uint AvailableSeats { get; set; } = uint.MaxValue;

    [Required(ErrorMessage = "Property CompetitiveSelection is required")]
    public bool CompetitiveSelection { get; set; } = default;
    
    [Required(ErrorMessage = "WorkshopType is required")]
    public WorkshopType WorkshopType { get; set; } = WorkshopType.None;
    
    [Required(ErrorMessage = "Type of age composition is required")]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;
    
    [Required(ErrorMessage = "Educational shift is required")]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;
    
    [Required(ErrorMessage = "Short stay is required")]
    public bool ShortStay { get; set; } = false;
    
    [Required(ErrorMessage = "Should be indicated if the Workshop operates with funds from parents or benefactors")]
    public bool IsSelfFinanced { get; set; } = false;
    
    [Required(ErrorMessage = "Form of learning is required")]
    public FormOfLearning FormOfLearning { get; set; }

    [Required(ErrorMessage = "Property IsPaid is required")]
    public bool IsPaid { get; set; } = false;
    
    // TODO: Return to this when languages are implemented
    [Required(ErrorMessage = "Language of education is required")]
    public uint LanguageOfEducationId { get; set; } = 1;

    #endregion

    [MaxLength(500)]
    public string CompetitiveSelectionDescription { get; set; }

    [Required(ErrorMessage = "Provider title is required")]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitle { get; set; } = string.Empty;

    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitleEn { get; set; } = string.Empty;

    public OwnershipType ProviderOwnership { get; set; }

    [MaxLength(Constants.MaxKeywordsLength)]
    public string Keywords { get; set; } = string.Empty;

    public WorkshopStatus Status { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal Price { get; set; } = default;

    [Required(ErrorMessage = "Type of pay rate is required")]
    public PayRateType PayRate { get; set; }

    public bool WithDisabilityOptions { get; set; } = default;

    [MaxLength(200)]
    public string DisabilityOptionsDesc { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property IsSpecial is required")]
    public bool IsSpecial { get; set; } = false;

    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    [Required(ErrorMessage = "Property IsInclusive is required")]
    public bool IsInclusive { get; set; } = false;

    [MaxLength(Constants.EnrollmentProcedureDescription)]
    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; } = false;

    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    public string PreferentialTermsOfParticipation { get; set; }

    public Coverage Coverage { get; set; } = Coverage.School;

    [Required(ErrorMessage = "Provider is required")]
    public Guid ProviderId { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public Guid? DefaultTeacherId { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    public Guid? ParentWorkshopId { get; set; }

    #region Contact information

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.MaxPhoneNumberLengthWithPlusSign)]
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

    [Required]
    public long AddressId { get; set; }

    #endregion

    #region Navigation properties

    public virtual Provider Provider { get; set; }

    public virtual InstitutionHierarchy InstitutionHierarchy { get; set; }

    public virtual Teacher DefaultTeacher { get; set; }

    public virtual Workshop ParentWorkshop { get; set; }

    public virtual ICollection<WorkshopDescriptionItem> WorkshopDescriptionItems { get; set; }

    public virtual ICollection<Workshop>
        IncludedStudyGroups { get; set; } // Navigation property to included study groups

    public virtual List<Employee> Employees { get; set; }

    public virtual List<Teacher> Teachers { get; set; }

    public virtual List<Application> Applications { get; set; }

    public virtual List<DateTimeRange> DateTimeRanges { get; set; }

    public virtual ICollection<ChatRoomWorkshop> ChatRooms { get; set; }

    public virtual List<Image<Workshop>> Images { get; set; }

    public virtual Address Address { get; set; }

    public virtual List<Tag> Tags { get; set; }

    #endregion
}