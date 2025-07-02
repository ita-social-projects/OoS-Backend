using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;

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
            MinimumAge = model.MinimumAge,
            MaximumAge = model.MaximumAge,
            Price = model.Price,
            CompetitiveSelection = model.CompetitiveSelection,
            Contacts = model.Contacts?.ToDto(),
            SubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(s => s.Id).ToList() ?? [],
            Coverage = model.Coverage?.ToDto(),        
        };

    public static List<CompetitiveEventV2Dto> ToV2Dto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
        => list.MapToList(ToV2Dto);

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
            OrganizerOfTheEventId = draft.CompetitiveEventDraftContent.OrganizerOfTheEventId,
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
            CoverageId = draft.CoverageId,
            CompetitiveEventAccountingTypeId = draft.CompetitiveEventAccountingTypeId,
            SubDirectionIds = draft.CompetitiveEvent?.SubDirections?.Select(s => s.Id).ToList() ?? []
        };
    }

    public static void SetToDraft(this CompetitiveEventV2Dto dto, OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
    {
        draft.ProviderId = dto.OrganizerOfTheEventId;
        draft.CompetitiveEventId = dto.Id == Guid.Empty ? (Guid?)null : dto.Id;
        draft.CompetitiveEventDraftContent = dto.ToDraftContent();
    }

    public static OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft ToDraft(this CompetitiveEventV2Dto competitiveEventV2Dto)
        => new()
        {
            ProviderId = competitiveEventV2Dto.OrganizerOfTheEventId,
            CompetitiveEventId = competitiveEventV2Dto.Id == Guid.Empty ? (Guid?)null : competitiveEventV2Dto.Id,
            CoverImageId = competitiveEventV2Dto.CoverImageId,
            CoverageId = competitiveEventV2Dto.CoverageId,
            CompetitiveEventAccountingTypeId = competitiveEventV2Dto.CompetitiveEventAccountingTypeId,
            CompetitiveEventDraftContent = competitiveEventV2Dto.ToDraftContent(),
        };

    public static List<OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft> ToDraft(this IEnumerable<CompetitiveEventV2Dto> list)
        => list.MapToList(ToDraft);

    public static CompetitiveEventDraftContent ToDraftContent(this CompetitiveEventV2Dto competitiveEventV2Dto)
        => new()
        {
            AdditionalDescription = competitiveEventV2Dto.AdditionalDescription,
            AreThereBenefits = competitiveEventV2Dto.AreThereBenefits ?? default,
            Benefits = competitiveEventV2Dto.Benefits,
            CompetitiveSelection = competitiveEventV2Dto.CompetitiveSelection ?? default,
            Contacts = competitiveEventV2Dto.Contacts?.ToModel() ?? new List<OutOfSchool.Services.Models.ContactInfo.Contacts>(),
            DescriptionOfOptionsForPeopleWithDisabilities = competitiveEventV2Dto.DescriptionOfOptionsForPeopleWithDisabilities,
            DescriptionOfTheEnrollmentProcedure = competitiveEventV2Dto.DescriptionOfTheEnrollmentProcedure,
            MaximumAge = competitiveEventV2Dto.MaximumAge ?? default,
            MinimumAge = competitiveEventV2Dto.MinimumAge,
            NumberOfSeats = competitiveEventV2Dto.NumberOfSeats,
            OptionsForPeopleWithDisabilities = competitiveEventV2Dto.OptionsForPeopleWithDisabilities ?? default,
            OrganizerOfTheEventId = competitiveEventV2Dto.OrganizerOfTheEventId,
            ParentId = competitiveEventV2Dto.ParentId,
            PlannedFormatOfClasses = competitiveEventV2Dto.PlannedFormatOfClasses ?? default,
            PreferentialTermsOfParticipation = competitiveEventV2Dto.PreferentialTermsOfParticipation,
            Price = competitiveEventV2Dto.Price ?? default,
            RegistrationEndTime = competitiveEventV2Dto.RegistrationEndTime ?? default,
            RegistrationStartTime = competitiveEventV2Dto.RegistrationStartTime ?? default,
            ScheduledEndTime = competitiveEventV2Dto.ScheduledEndTime,
            ScheduledStartTime = competitiveEventV2Dto.ScheduledStartTime,
            ShortTitle = competitiveEventV2Dto.ShortTitle,
            Title = competitiveEventV2Dto.Title,
            TermsOfParticipation = competitiveEventV2Dto.TermsOfParticipation
        };
}
