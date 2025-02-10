using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventBaseDto: IHasCoverImage, IHasImages, IHasContactsDto<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent>
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxCompetitiveEventTitleLength)]
    [MinLength(Constants.MinCompetitiveEventTitleLength)]
    public string Title { get; set; }

    [Required(ErrorMessage = "ShortTitle is required")]
    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxCompetitiveEventShortTitleLength)]
    [MinLength(Constants.MinCompetitiveEventShortTitleLength)]
    public string ShortTitle { get; set; }

    [Required]
    [EnumDataType(typeof(CompetitiveEventStates), ErrorMessage = Constants.EnumErrorMessage)]
    public CompetitiveEventStates State { get; set; } = CompetitiveEventStates.Draft;

    public DateTimeOffset? RegistrationStartTime { get; set; }

    public DateTimeOffset? RegistrationEndTime { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? BuildingHoldingId { get; set; }

    public Guid? ChildParticipantId { get; set; }

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
    public int CompetitiveEventAccountingTypeId { get; set; }

    [MaxLength(Constants.MaxDescriptionLength)]
    public string Description { get; set; }

    [MaxLength(Constants.EnrollmentProcedureDescription)]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    public Guid? OrganizerOfTheEventId { get; set; }

    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning? PlannedFormatOfClasses { get; set; }

    public Guid? VenueId { get; set; }

    [MaxLength(Constants.MaxVenueNameLength)]
    public string VenueName { get; set; }

    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    public string TermsOfParticipation { get; set; }

    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    public string PreferentialTermsOfParticipation { get; set; }

    public List<JudgeDto> Judges { get; set; }

    public bool? AreThereBenefits { get; set; }

    [MaxLength(Constants.MaxBenefitsLength)]
    public string Benefits { get; set; }

    public bool? OptionsForPeopleWithDisabilities { get; set; }

    [MaxLength(Constants.DisabilityOptionsLength)]
    public string DescriptionOfOptionsForPeopleWithDisabilities { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int? MaximumAge { get; set; }

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public int? Price { get; set; }

    public bool? CompetitiveSelection { get; set; }

    public uint? NumberOfOccupiedSeats { get; set; }

    public List<Guid> ParticipantsOfTheEvent { get; set; } = new List<Guid>();

    public string CoverImageId { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    public IList<string> ImageIds { get; set; } = new List<string>();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<ContactsDto> Contacts { get; set; }
}