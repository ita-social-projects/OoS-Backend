using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.Exported.Contacts;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Exported.CompetitiveEvents;

public class CompetitiveEventInfoDto : CompetitiveEventInfoBaseDto, IExternalRatingInfo
{
    [Required]
    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxCompetitiveEventTitleLength)]
    [MinLength(Constants.MinCompetitiveEventTitleLength)]
    public string Title { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxCompetitiveEventShortTitleLength)]
    [MinLength(Constants.MinCompetitiveEventShortTitleLength)]
    public string ShortTitle { get; set; }

    [Required] public DateTimeOffset ScheduledStartTime { get; set; }

    [Required] public DateTimeOffset ScheduledEndTime { get; set; }

    [Required] public uint NumberOfSeats { get; set; } = uint.MaxValue;

    public uint? NumberOfOccupiedSeats { get; set; }

    public Guid OrganizerOfTheEventId { get; set; }

    public float Rating { get; set; } = 0;
    public int NumberOfRatings { get; set; } = 0;
    
    //public string Institution { get; set; }

    //public string InstitutionHierarchy { get; set; }

    //public List<long> DirectionIds { get; set; }
    
    public List<long> SubDirectionIds { get; set; }

    public string SubDirections { get; set; }

    public CoverageInfoDto Coverage { get; set; }

    public DateTimeOffset? RegistrationStartTime { get; set; }

    public DateTimeOffset? RegistrationEndTime { get; set; }

    public Guid? ParentEventId { get; set; }

    public Guid? BuildingHoldingId { get; set; }

    public List<CompetitiveEventDescriptionItemDto> CompetitiveEventDescriptionItems { get; set; }

    public AccountingTypeInfoDto AccountingType { get; set; }

    [MaxLength(Constants.EnrollmentProcedureDescription)]
    public string DescriptionOfTheEnrollmentProcedure { get; set; } = string.Empty;

    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning PlannedFormatOfClasses { get; set; }

    [MaxLength(Constants.MaxVenueNameLength)]
    public string VenueName { get; set; } = string.Empty;

    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    public string TermsOfParticipation { get; set; } = string.Empty;

    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    public string PreferentialTermsOfParticipation { get; set; } = string.Empty;

    public bool AreThereBenefits { get; set; }

    [MaxLength(Constants.MaxBenefitsLength)]
    public string Benefits { get; set; } = string.Empty;

    public bool OptionsForPeopleWithDisabilities { get; set; }

    [MaxLength(Constants.DisabilityOptionsLength)]
    public string DescriptionOfOptionsForPeopleWithDisabilities { get; set; }

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaximumAge { get; set; } = 0;

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public int Price { get; set; } = 0;

    public bool CompetitiveSelection { get; set; }

    [MaxLength(2000)]
    public string CompetitiveSelectionDescription { get; set; }

    public string CoverImageId { get; set; } = string.Empty;

    public List<string> ImageIds { get; set; } = [];

    public List<ContactsInfoDto> Contacts { get; set; }
}