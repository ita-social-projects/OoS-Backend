using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Workshops;

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
            Contacts = model.Contacts?.ToDto(),
            SubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(s => s.Id).ToList() ?? [],
            Coverage = model.Coverage?.ToDto(),
        };

    public static CompetitiveEventV2Dto ToDto(this OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
    {
        return new CompetitiveEventV2Dto()
        {
            Id = draft.CompetitiveEventId ?? default,
            Title = draft.CompetitiveEventDraftContent?.Title,
            ShortTitle = draft.CompetitiveEventDraftContent?.ShortTitle,
            RegistrationStartTime = draft.CompetitiveEventDraftContent?.RegistrationStartTime,
            RegistrationEndTime = draft.CompetitiveEventDraftContent?.RegistrationEndTime,
            ParentId = draft.CompetitiveEventDraftContent?.ParentId,
            AdditionalDescription = draft.CompetitiveEventDraftContent?.AdditionalDescription,
            ScheduledStartTime = draft.CompetitiveEventDraftContent?.ScheduledStartTime ?? default,
            ScheduledEndTime = draft.CompetitiveEventDraftContent?.ScheduledEndTime ?? default,
            NumberOfSeats = draft.CompetitiveEventDraftContent?.NumberOfSeats ?? default,
            DescriptionOfTheEnrollmentProcedure = draft.CompetitiveEventDraftContent?.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = draft.CompetitiveEventDraftContent?.OrganizerOfTheEventId ?? default,
            PlannedFormatOfClasses = draft.CompetitiveEventDraftContent?.PlannedFormatOfClasses,
            VenueName = draft.CompetitiveEventDraftContent?.VenueName,
            TermsOfParticipation = draft.CompetitiveEventDraftContent?.TermsOfParticipation,
            PreferentialTermsOfParticipation = draft.CompetitiveEventDraftContent?.PreferentialTermsOfParticipation,
            AreThereBenefits = draft.CompetitiveEventDraftContent?.AreThereBenefits,
            Benefits = draft.CompetitiveEventDraftContent?.Benefits,
            OptionsForPeopleWithDisabilities = draft.CompetitiveEventDraftContent?.OptionsForPeopleWithDisabilities,
            DescriptionOfOptionsForPeopleWithDisabilities = draft.CompetitiveEventDraftContent?.DescriptionOfOptionsForPeopleWithDisabilities,
            MinimumAge = draft.CompetitiveEventDraftContent?.MinimumAge ?? 0,
            MaximumAge = draft.CompetitiveEventDraftContent?.MaximumAge,
            Price = draft.CompetitiveEventDraftContent?.Price,
            CompetitiveSelection = draft.CompetitiveEventDraftContent?.CompetitiveSelection,
            Contacts = draft.CompetitiveEventDraftContent?.Contacts?.ToDto() ?? new List<ContactsDto>(),
            CoverImageId = draft.CoverImageId,
            ImageIds = draft.Images?.Select(x => x.ExternalStorageId).ToList() ?? new List<string>(),
        };
    }


    public static List<CompetitiveEventV2Dto> ToV2Dto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
        => list.MapToList(ToV2Dto);
}
