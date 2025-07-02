using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
public class CompetitiveEventV2CreateRequestDto : CompetitiveEventCreateUpdateDto
{
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }
}

public static class CompetitiveEventV2CreateRequestDtoExtensions
{
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent ToModel(this CompetitiveEventV2CreateRequestDto dto)
        => new()
        {
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            State = dto.State,
            RegistrationStartTime = dto.RegistrationStartTime ?? default,
            RegistrationEndTime = dto.RegistrationEndTime ?? default,
            ParentId = dto.ParentId,
            CoverageId = dto.CoverageId,
            AdditionalDescription = dto.AdditionalDescription,
            ScheduledStartTime = dto.ScheduledStartTime,
            ScheduledEndTime = dto.ScheduledEndTime,
            NumberOfSeats = dto.NumberOfSeats,
            CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId,
            DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = dto.OrganizerOfTheEventId,
            PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? default,
            VenueName = dto.VenueName,
            TermsOfParticipation = dto.TermsOfParticipation,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
            AreThereBenefits = dto.AreThereBenefits ?? false,
            Benefits = dto.Benefits,
            OptionsForPeopleWithDisabilities = dto.OptionsForPeopleWithDisabilities ?? false,
            DescriptionOfOptionsForPeopleWithDisabilities = dto.DescriptionOfOptionsForPeopleWithDisabilities,
            MinimumAge = dto.MinimumAge,
            MaximumAge = dto.MaximumAge ?? 0,
            Price = dto.Price ?? 0,
            CompetitiveSelection = dto.CompetitiveSelection ?? false,
            Contacts = dto.Contacts?.ToModel(),            
        };

    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent SetToModel(this CompetitiveEventV2CreateRequestDto dto, OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
    {
        model.Title = dto.Title;
        model.ShortTitle = dto.ShortTitle;
        model.State = dto.State;
        model.RegistrationStartTime = dto.RegistrationStartTime ?? model.RegistrationStartTime;
        model.RegistrationEndTime = dto.RegistrationEndTime ?? model.RegistrationEndTime;
        model.ParentId = dto.ParentId;
        model.CoverageId = dto.CoverageId;
        model.AdditionalDescription = dto.AdditionalDescription;
        model.ScheduledStartTime = dto.ScheduledStartTime;
        model.ScheduledEndTime = dto.ScheduledEndTime;
        model.NumberOfSeats = dto.NumberOfSeats;
        model.CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId;
        model.DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure;
        model.OrganizerOfTheEventId = dto.OrganizerOfTheEventId;
        model.PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? model.PlannedFormatOfClasses;
        model.VenueName = dto.VenueName;
        model.TermsOfParticipation = dto.TermsOfParticipation;
        model.PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation;
        model.AreThereBenefits = dto.AreThereBenefits ?? model.AreThereBenefits;
        model.Benefits = dto.Benefits;
        model.OptionsForPeopleWithDisabilities = dto.OptionsForPeopleWithDisabilities ?? model.OptionsForPeopleWithDisabilities;
        model.DescriptionOfOptionsForPeopleWithDisabilities = dto.DescriptionOfOptionsForPeopleWithDisabilities;
        model.MinimumAge = dto.MinimumAge;
        model.MaximumAge = dto.MaximumAge ?? model.MaximumAge;
        model.Price = dto.Price ?? model.Price;
        model.CompetitiveSelection = dto.CompetitiveSelection ?? model.CompetitiveSelection;
        model.Contacts = dto.Contacts?.ToModel() ?? model.Contacts;

        return model;
    }

    public static CompetitiveEventV2CreateRequestDto ToV2CreateRequestDto(this OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
       => new()
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
           Contacts = draft.CompetitiveEventDraftContent?.Contacts?.Any() == true
            ? draft.CompetitiveEventDraftContent.Contacts.ToDto()
            : draft.CompetitiveEvent?.Contacts?.ToDto() ?? new List<ContactsDto>(),
           CoverImageId = draft.CoverImageId,
           CoverageId = draft.CoverageId,
           CompetitiveEventAccountingTypeId = draft.CompetitiveEventAccountingTypeId,
           ImageIds = draft.Images?.Select(x => x.ExternalStorageId).ToList() ?? new List<string>(),
           SubDirectionIds = draft.CompetitiveEvent?.SubDirections?.Select(sd => sd.Id).ToList() ?? []
       };
}