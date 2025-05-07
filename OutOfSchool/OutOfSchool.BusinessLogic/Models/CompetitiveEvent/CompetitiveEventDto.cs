using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDto : CompetitiveEventBaseDto
{
    public bool IsDeleted { get; set; }
    public uint Rating { get; set; } = 0;
    public uint NumberOfRatings { get; set; } = 0;

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;
    public IList<string> ImageIds { get; set; }
    public CompetitiveEventCoverageDto Coverage { get; set; }
}

public static class CompetitiveEventDtoExtensions
{
    public static CompetitiveEventDto ToDto(this OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            State = model.State,
            RegistrationStartTime = model.RegistrationStartTime,
            RegistrationEndTime = model.RegistrationEndTime,
            ParentId = model.ParentId,
            BuildingHoldingId = model.BuildingHoldingId,
            ChildParticipantId = model.ChildParticipantId,
            CoverageId = model.CoverageId,
            CompetitiveEventDescriptionItems = model.CompetitiveEventDescriptionItems?.ToDto(),
            AdditionalDescription = model.AdditionalDescription,
            ScheduledStartTime = model.ScheduledStartTime,
            ScheduledEndTime = model.ScheduledEndTime,
            NumberOfSeats = model.NumberOfSeats,
            CompetitiveEventAccountingTypeId = model.CompetitiveEventAccountingTypeId,
            DescriptionOfTheEnrollmentProcedure = model.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = model.OrganizerOfTheEventId,
            PlannedFormatOfClasses = model.PlannedFormatOfClasses,
            VenueId = model.VenueId,
            VenueName = model.VenueName,
            TermsOfParticipation = model.TermsOfParticipation,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,
            AreThereBenefits = model.AreThereBenefits,
            Benefits = model.Benefits,
            OptionsForPeopleWithDisabilities = model.OptionsForPeopleWithDisabilities,
            DescriptionOfOptionsForPeopleWithDisabilities = model.DescriptionOfOptionsForPeopleWithDisabilities,
            MinimumAge = model.MinimumAge,
            MaximumAge = model.MaximumAge,
            Price = model.Price,
            CompetitiveSelection = model.CompetitiveSelection,
            NumberOfOccupiedSeats = model.NumberOfOccupiedSeats,
            Contacts = model.Contacts?.ToDto(),
            SubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(s => s.Id).ToList() ?? [],
            Coverage = model.Coverage?.ToDto(),
        };

    public static List<CompetitiveEventDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
        => list.MapToList(ToDto);
}