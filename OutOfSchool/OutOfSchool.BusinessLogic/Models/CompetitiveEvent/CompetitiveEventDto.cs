using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDto
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }

    [Required(ErrorMessage = "ShortTitle is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string ShortTitle { get; set; }

    [Required]
    [EnumDataType(typeof(CompetitiveEventStates), ErrorMessage = Constants.EnumErrorMessage)]
    public CompetitiveEventStates State { get; set; } = CompetitiveEventStates.Draft;

    public DateTimeOffset RegistrationStartTime { get; set; }

    public DateTimeOffset RegistrationEndTime { get; set; }

    public Guid ParentId { get; set; }

    public Guid BuildingHoldingId { get; set; }

    public Guid ChildParticipantId { get; set; }

    public Guid ChiefJudgeId { get; set; }

    public List<CompetitiveEventDescriptionItemDto> CompetitiveEventDescriptionItems { get; set; }

    [MaxLength(2000)]
    public string AdditionalDescription { get; set; }

    [Required]
    public DateTimeOffset ScheduledStartTime { get; set; }

    [Required]
    public DateTimeOffset ScheduledEndTime { get; set; }

    [Required]
    public uint NumberOfSeats { get; set; } = uint.MaxValue;

    [Required]
    public List<CompetitiveEventAccountingTypeDto> AccountingTypeOfEvent { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    [MaxLength(2000)]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    [Required]
    public Guid OrganizerOfTheEventId { get; set; }

    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning PlannedFormatOfClasses { get; set; }

    public Guid VenueId { get; set; }

    [MaxLength(2000)]
    public string PreferentialTermsOfParticipation { get; set; }

    //public virtual List<Individual> Judges { get; set; }

    public List<ProviderDto> ParticipantsOfTheEvent { get; set; }

    public bool AreThereBenefits { get; set; }

    public uint Rating { get; set; }

    public uint NumberOfRatings { get; set; }

    public bool OptionsForPeopleWithDisabilities { get; set; }

    [MaxLength(2000)]
    public string DescriptionOfOptionsForPeopleWithDisabilities { get; set; }

    public long CategoryId { get; set; }

    [MaxLength(250)]
    public string Subcategory { get; set; }

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaximumAge { get; set; }

    public List<CompetitiveEventCoverageDto> Coverage { get; set; }

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public int Price { get; set; } = default;

    public bool CompetitiveSelection { get; set; }

    public uint NumberOfOccupiedSeats { get; set; }
}
