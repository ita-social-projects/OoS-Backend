using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.Exported.Contacts;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.CompetitiveEvent;

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

    public List<long> DirectionIds { get; set; }
    
    public List<long> SubDirectionIds { get; set; }

    public CoverageInfoDto Coverage { get; set; }

    public DateTimeOffset? RegistrationStartTime { get; set; }

    public DateTimeOffset? RegistrationEndTime { get; set; }

    public Guid? ParentEventId { get; set; }

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

public static class CompetitiveEventInfoDtoExtensions
{
    public static CompetitiveEventInfoBaseDto ToBaseInfoDto(this OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.State == CompetitiveEventStates.Archived,
        };

    public static CompetitiveEventInfoDto ToInfoDto(this OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            ScheduledStartTime = model.ScheduledStartTime,
            ScheduledEndTime = model.ScheduledEndTime,
            NumberOfSeats = model.NumberOfSeats,
            NumberOfOccupiedSeats = 0,
            OrganizerOfTheEventId = model.OrganizerOfTheEventId,
            DirectionIds = model.SubDirections?.Where(x => !x.IsDeleted && !x.Direction.IsDeleted).Select(d => d.DirectionId).Distinct().ToList() ?? [],
            SubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(s => s.Id).ToList() ?? [],
            Coverage = model.Coverage?.ToInfoDto(),
            RegistrationStartTime = model.RegistrationStartTime,
            RegistrationEndTime = model.RegistrationEndTime,
            ParentEventId = model.ParentId,
            CompetitiveEventDescriptionItems = model.CompetitiveEventDescriptionItems?.ToDto(),
            AccountingType = model.CompetitiveEventAccountingType?.ToInfoDto(),
            DescriptionOfTheEnrollmentProcedure = model.DescriptionOfTheEnrollmentProcedure,
            PlannedFormatOfClasses = model.PlannedFormatOfClasses,
            VenueName = model.VenueName,
            TermsOfParticipation = model.TermsOfParticipation,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,
            AreThereBenefits = model.AreThereBenefits,
            Benefits = model.Benefits,
            MinimumAge = model.MinimumAge,
            MaximumAge = model.MaximumAge,
            Price = model.Price,
            CompetitiveSelection = model.CompetitiveSelection,
            CompetitiveSelectionDescription = model.AdditionalDescription,
            Contacts = model.Contacts?.ToInfoDto(),
            CoverImageId = model.CoverImageId,
            ImageIds = model.Images?.Select(i => i.ExternalStorageId).ToList() ?? []
        };

    public static List<CompetitiveEventInfoBaseDto> ToBaseOrInfoDto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
        => list.MapToList(x => x.State == CompetitiveEventStates.Archived ? x.ToBaseInfoDto() : x.ToInfoDto());
}