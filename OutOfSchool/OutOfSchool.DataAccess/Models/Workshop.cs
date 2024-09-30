using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class Workshop : BusinessEntity, IImageDependentEntity<Workshop>, IHasEntityImages<Workshop>
{
    // 1
    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; } = string.Empty;

    // 2
    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    // 3
    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    // 4
    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    // 5
    public bool CompetitiveSelection { get; set; } = false;

    // 6
    [MaxLength(500)]
    public string CompetitiveSelectionDescription { get; set; }

    // 7
    [Required(ErrorMessage = "Provider title is required")]
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitle { get; set; } = string.Empty;

    // 8
    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitleEn { get; set; } = string.Empty;

    // 9
    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    // 10
    [MaxLength(200)]
    public string Keywords { get; set; } = string.Empty;

    // 11
    public WorkshopStatus Status { get; set; }

    // 12
    public uint AvailableSeats { get; set; } = uint.MaxValue;

    // 13
    [Required]
    public FormOfLearning FormOfLearning { get; set; }

    // 14
    public bool IsPaid { get; set; } = false;

    // 15)
    [Required(ErrorMessage = "Type of pay rate is required")]
    public PayRateType PayRate { get; set; }

    // 16
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal Price { get; set; } = default;

    // 17
    [Required(ErrorMessage = "Short stay is required")]
    public bool ShortStay { get; set; } = false;

    // 18
    [Required(ErrorMessage = "Should be indicated if the Workshop operates with funds from parents or benefactors")]
    public bool IsSelfFinanced { get; set; } = false;

    // 19
    [Required(ErrorMessage = "Property IsSpecial is required")]
    public bool IsSpecial { get; set; } = false;

    // 20
    [MaxLength(500)]
    public string AdditionalDescription { get; set; }

    // 21
    public bool AreThereBenefits { get; set; } = false;

    // 22
    [MaxLength(500)]
    public string PreferentialTermsOfParticipation { get; set; }

    // 23
    public bool WithDisabilityOptions { get; set; } = default;

    // 24
    public uint DisabilityOptionsDescId { get; set; } = uint.MinValue;

    // 25
    [Required(ErrorMessage = "Educational shift is required")]
    public uint EducationalShiftId { get; set; } = uint.MinValue;

    // 26
    [Required(ErrorMessage = "Language of education is required")]
    public uint LanguageOfEducationId { get; set; } = uint.MinValue;

    // 27
    [Required(ErrorMessage = "Type of age composition is required")]
    public uint TypeOfAgeCompositionId { get; set; } = uint.MinValue;

    // 28
    [Required(ErrorMessage = "Educational disciplines is required")]
    public Guid EducationalDisciplines { get; set; } = Guid.Empty;

    // 29
    [Required(ErrorMessage = "Category is required")]
    public uint CategoryId { get; set; } = uint.MinValue;

    // 30
    [Required(ErrorMessage = "GropeType is required")]
    public uint GropeTypeId { get; set; } = uint.MinValue;

    // 31
    public uint CoverageId { get; set; } = uint.MinValue;

    // 32
    [Required(ErrorMessage = "Provider is required")]
    public Guid ProviderId { get; set; }

    // 33
    public Guid? InstitutionHierarchyId { get; set; }

    // 34
    public Guid? TeacherId { get; set; }

    // 35
    [Required(ErrorMessage = "Cover image is required")]
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    // 36
    [Required(ErrorMessage = "Contact is required")]
    public Guid ContactId { get; set; }

    // 37
    [Required(ErrorMessage = "Contact information is required")]
    public uint ContactInformationId { get; set; } = uint.MinValue;

    // 38
    public Guid? MemberOfWorkshopId { get; set; }

    // 39
    public virtual Provider Provider { get; set; }

    // 40
    public virtual InstitutionHierarchy InstitutionHierarchy { get; set; }

    // 41
    public virtual Contact Contact { get; set; }

    // 42
    public virtual ContactInformation ContactInformation { get; set; }

    // 43
    public virtual ICollection<WorkshopDescriptionItem> WorkshopDescriptionItems { get; set; }

    // 44
    public virtual List<Workshop> IncludedStudyGroups { get; set; }

    // 45
    public virtual List<ProviderAdmin> ProviderAdmins { get; set; }

    // 46
    public virtual List<Teacher> Teachers { get; set; }

    // 47
    public virtual List<Application> Applications { get; set; }

    // 48
    public virtual List<DateTimeRange> DateTimeRanges { get; set; }

    // 49
    public virtual ICollection<ChatRoomWorkshop> ChatRooms { get; set; }

    // 50
    public virtual List<Image<Workshop>> Images { get; set; }
}