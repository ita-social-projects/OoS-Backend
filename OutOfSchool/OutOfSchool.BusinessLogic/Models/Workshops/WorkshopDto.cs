using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Models.Teachers;
using OutOfSchool.BusinessLogic.Models.Workshops.V2;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

[JsonDerivedType(typeof(WorkshopV2Dto))]
public class WorkshopDto : IHasRating
{
    public Guid Id { get; set; }

    public string Title { get; set; }
    
    public string ShortTitle { get; set; }

    public string Phone { get; set; }

    public string Email { get; set; }

    public string Website { get; set; }

    public string Facebook { get; set; }

    public string Instagram { get; set; }
    
    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public List<DateTimeRangeDto> DateTimeRanges { get; set; }

    public bool IsPaid { get; set; }

    public decimal? Price { get; set; }

    public PayRateType? PayRate { get; set; }

    public FormOfLearning FormOfLearning { get; set; }

    public uint? AvailableSeats { get; set; }
    
    public uint TakenSeats { get; set; }

    public bool CompetitiveSelection { get; set; }

    public string CompetitiveSelectionDescription { get; set; }

    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }

    public bool WithDisabilityOptions { get; set; }

    public string DisabilityOptionsDesc { get; set; }

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public string InstitutionHierarchy { get; set; }

    public TeacherDto DefaultTeacher { get; set; }

    public List<long> DirectionIds { get; set; }

    public IEnumerable<string> Keywords { get; set; }

    public List<TeacherDto> Teachers { get; set; }

    public Guid ProviderId { get; set; }

    public string ProviderTitle { get; set; }

    public string ProviderTitleEn { get; set; }

    public ProviderLicenseStatus ProviderLicenseStatus { get; set; } 

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool ShortStay { get; set; }

    public bool IsSelfFinanced { get; set; }

    public bool IsSpecial { get; set; }

    public SpecialNeedsType SpecialNeedsType { get; set; }

    public bool IsInclusive { get; set; }

    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; }

    public string PreferentialTermsOfParticipation { get; set; }

    public EducationalShift EducationalShift { get; set; }

    public uint LanguageOfEducationId { get; set; }

    public AgeComposition AgeComposition { get; set; }

    public Coverage Coverage { get; set; }
    
    public WorkshopType WorkshopType { get; set; }

    public Guid? DefaultTeacherId { get; set; }

    public Guid? ParentWorkshopId { get; set; }

    public long AddressId { get; set; }

    public AddressDto Address { get; set; }
    
    public List<ContactsDto> Contacts { get; set; }

    public float Rating { get; set; }
    
    public int NumberOfRatings { get; set; }

    public List<TagDto> Tags { get; set; }

    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    public ProviderStatus ProviderStatus { get; set; } = ProviderStatus.Pending;
    
    [JsonIgnore]
    public bool IsBlocked { get; set; }
}