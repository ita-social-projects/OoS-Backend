using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

// TODO:
// - Add educational disciplines (many ED to 1 workshop)
// - Add language
public class Workshop : BusinessEntity, IImageDependentEntity<Workshop>, IHasEntityImages<Workshop>, IHasContacts, IHasHiddenFields
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
    
    public WorkshopType WorkshopType { get; set; } = WorkshopType.Workshop;
    
    [Required(ErrorMessage = "Type of age composition is required")]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;
    
    [Required(ErrorMessage = "Educational shift is required")]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;
    
    [Required(ErrorMessage = "Should be indicated if the Workshop operates with funds from parents or benefactors")]
    public bool IsSelfFinanced { get; set; } = false;
    
    [Required(ErrorMessage = "Form of learning is required")]
    public FormOfLearning FormOfLearning { get; set; }

    [Required(ErrorMessage = "Property IsPaid is required")]
    public bool IsPaid { get; set; } = false;

    [Required]
    public long LanguageOfEducationId { get; set; }

    [Required(ErrorMessage = "Study period start date is required")]
    public DateOnly StudyPeriodStartDate { get; set; }

    [Required(ErrorMessage = "Study period end date is required")]
    public DateOnly StudyPeriodEndDate { get; set; }

    #endregion

    [MaxLength(Constants.MaxCompetitiveSelectionDescriptionLength)]
    public string CompetitiveSelectionDescription { get; set; }

    public OwnershipType ProviderOwnership { get; set; }

    [MaxLength(Constants.MaxKeywordsLength)]
    public string Keywords { get; set; } = string.Empty;

    public WorkshopStatus Status { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal Price { get; set; } = default;

    [Required(ErrorMessage = "Type of pay rate is required")]
    public PayRateType PayRate { get; set; }
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    [Required(ErrorMessage = "Property IsInclusive is required")]
    public bool IsInclusive { get; set; } = false;

    [MaxLength(Constants.EnrollmentProcedureDescription)]
    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; } = false;

    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    public string PreferentialTermsOfParticipation { get; set; }

    public Coverage Coverage { get; set; } = Coverage.School;

    public bool IsChampionPath { get; set; } = false;

    [Required(ErrorMessage = "Provider is required")]
    public Guid ProviderId { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public Guid? DefaultTeacherId { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    public Guid? ParentWorkshopId { get; set; }

    #region Navigation properties

    public virtual Provider Provider { get; set; }

    public virtual InstitutionHierarchy InstitutionHierarchy { get; set; }

    public virtual Teacher DefaultTeacher { get; set; }

    public virtual Workshop ParentWorkshop { get; set; }

    public virtual ICollection<WorkshopDescriptionItem> WorkshopDescriptionItems { get; set; }

    public virtual ICollection<Workshop>
        IncludedStudyGroups { get; set; } // Navigation property to included study groups

    public virtual List<Teacher> Teachers { get; set; }

    public virtual List<Application> Applications { get; set; }

    public virtual List<DateTimeRange> DateTimeRanges { get; set; }

    public virtual ICollection<ChatRoomWorkshop> ChatRooms { get; set; }

    public virtual List<Image<Workshop>> Images { get; set; }

    public virtual List<Tag> Tags { get; set; }

    public virtual List<StudySubject> StudySubjects { get; set; }
    
    public virtual Language LanguageOfEducation { get; set; }
    #endregion

    #region Owned entities

    public List<Contacts> Contacts { get; set; } = [];

    #endregion
    
    #region Minsport external API integration

    /// <summary>
    /// ID of the section in the Ministry of Sport Registry (UUID).
    /// </summary>
    public Guid? MinsportSectionId { get; set; }

    #endregion
}