using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
public class CompetitiveEventV2Dto : CompetitiveEventDto, IHasCoverImage, IHasImages
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }
}

public static class CompetitiveEventV2DtoExtensions
{
    public static CompetitiveEventV2Dto ToV2Dto(this OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
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

    public static List<CompetitiveEventV2Dto> ToV2Dto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
        => list.MapToList(ToV2Dto);
}
