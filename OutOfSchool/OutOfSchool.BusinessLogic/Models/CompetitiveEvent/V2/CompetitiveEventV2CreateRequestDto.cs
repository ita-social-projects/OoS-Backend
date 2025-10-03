using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
public class CompetitiveEventV2CreateRequestDto : CompetitiveEventBaseDto
{
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from CompetitiveEventBaseDto
        foreach (var error in base.Validate(validationContext))
            yield return error;

        if ((CoverImage is null) == string.IsNullOrEmpty(CoverImageId))
        {
            yield return new ValidationResult(
                "Must be filled either CoverImage or CoverImageId, but not both",
                [nameof(CoverImage), nameof(CoverImageId)]);
        }

        if ((ImageFiles ?? []).Count == 0 && (ImageIds ?? []).Count == 0)
        {
            yield return new ValidationResult(
            "At least one of the ImageFiles or ImageIds fields must be filled in.",
            [nameof(ImageFiles), nameof(ImageIds)]);
        }
    }
}

public static class CompetitiveEventV2CreateRequestDtoExtensions
{
    /// <summary>
        /// Creates a new CompetitiveEvent domain model populated from the DTO.
        /// </summary>
        /// <remarks>
        /// Nullable DTO fields are converted with sensible defaults: null registration times and <see cref="PlannedFormatOfClasses"/> are set to their default values; <see cref="AreThereBenefits"/>, <see cref="CompetitiveSelection"/> default to false; <see cref="MaximumAge"/> and <see cref="Price"/> default to 0. Contacts are mapped via <c>dto.Contacts?.ToModel()</c>. The DTO's <see cref="CoverImageId"/> is copied to the model.
        /// </remarks>
        /// <returns>A new <see cref="OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent"/> instance with properties copied from the DTO.</returns>
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
            ScheduledStartTime = dto.ScheduledStartTime,
            ScheduledEndTime = dto.ScheduledEndTime,
            NumberOfSeats = dto.NumberOfSeats,
            CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId,
            DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = dto.OrganizerOfTheEventId,
            PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? default,
            VenueName = dto.VenueName,
            TermsOfParticipation = dto.TermsOfParticipation,
            AreThereBenefits = dto.AreThereBenefits ?? false,
            Benefits = dto.Benefits,
            MinimumAge = dto.MinimumAge,
            MaximumAge = dto.MaximumAge ?? 0,
            Price = dto.Price ?? 0,
            CompetitiveSelection = dto.CompetitiveSelection ?? false,
            Contacts = dto.Contacts?.ToModel(),   
            CoverImageId = dto.CoverImageId,
        };

    /// <summary>
    /// Copies values from a CompetitiveEventV2CreateRequestDto into an existing CompetitiveEvent domain model, updating the model in-place.
    /// </summary>
    /// <param name="dto">Source DTO containing new values; nullable properties on the DTO will not overwrite existing model values.</param>
    /// <param name="model">The existing domain model to update.</param>
    /// <returns>The same CompetitiveEvent instance after applying updates.</returns>
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent SetToModel(this CompetitiveEventV2CreateRequestDto dto, OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
    {
        model.Title = dto.Title;
        model.ShortTitle = dto.ShortTitle;
        model.State = dto.State;
        model.RegistrationStartTime = dto.RegistrationStartTime ?? model.RegistrationStartTime;
        model.RegistrationEndTime = dto.RegistrationEndTime ?? model.RegistrationEndTime;
        model.ParentId = dto.ParentId;
        model.CoverageId = dto.CoverageId;
        model.ScheduledStartTime = dto.ScheduledStartTime;
        model.ScheduledEndTime = dto.ScheduledEndTime;
        model.NumberOfSeats = dto.NumberOfSeats;
        model.CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId;
        model.DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure;
        model.OrganizerOfTheEventId = dto.OrganizerOfTheEventId;
        model.PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? model.PlannedFormatOfClasses;
        model.VenueName = dto.VenueName;
        model.TermsOfParticipation = dto.TermsOfParticipation;
        model.AreThereBenefits = dto.AreThereBenefits ?? model.AreThereBenefits;
        model.Benefits = dto.Benefits;
        model.MinimumAge = dto.MinimumAge;
        model.MaximumAge = dto.MaximumAge ?? model.MaximumAge;
        model.Price = dto.Price ?? model.Price;
        model.CompetitiveSelection = dto.CompetitiveSelection ?? model.CompetitiveSelection;
        model.Contacts = dto.Contacts?.ToModel() ?? model.Contacts;

        return model;
    }

    /// <summary>
       /// Converts a <see cref="OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft"/> into a <see cref="CompetitiveEventV2CreateRequestDto"/>.
       /// </summary>
       /// <param name="draft">The draft to convert; draft content and related collections are read to populate the DTO. Nullable draft content and collections are handled with sensible defaults (empty lists, zeros, or preserved defaults) where appropriate.</param>
       /// <returns>A new <see cref="CompetitiveEventV2CreateRequestDto"/> populated from the draft. Lists such as ImageIds and Contacts are returned as empty lists when the draft provides no values.</returns>
       public static CompetitiveEventV2CreateRequestDto ToV2CreateRequestDto(this OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
       => new()
       {
           Id = draft.CompetitiveEventId ?? default,
           Title = draft.CompetitiveEventDraftContent?.Title,
           ShortTitle = draft.CompetitiveEventDraftContent?.ShortTitle,
           RegistrationStartTime = draft.CompetitiveEventDraftContent?.RegistrationStartTime,
           RegistrationEndTime = draft.CompetitiveEventDraftContent?.RegistrationEndTime,
           ParentId = draft.CompetitiveEventDraftContent?.ParentId,
           ScheduledStartTime = draft.CompetitiveEventDraftContent?.ScheduledStartTime ?? default,
           ScheduledEndTime = draft.CompetitiveEventDraftContent?.ScheduledEndTime ?? default,
           NumberOfSeats = draft.CompetitiveEventDraftContent?.NumberOfSeats ?? default,
           DescriptionOfTheEnrollmentProcedure = draft.CompetitiveEventDraftContent?.DescriptionOfTheEnrollmentProcedure,
           OrganizerOfTheEventId = draft.CompetitiveEventDraftContent?.OrganizerOfTheEventId ?? default,
           PlannedFormatOfClasses = draft.CompetitiveEventDraftContent?.PlannedFormatOfClasses,
           VenueName = draft.CompetitiveEventDraftContent?.VenueName,
           TermsOfParticipation = draft.CompetitiveEventDraftContent?.TermsOfParticipation,
           AreThereBenefits = draft.CompetitiveEventDraftContent?.AreThereBenefits,
           Benefits = draft.CompetitiveEventDraftContent?.Benefits,
           MinimumAge = draft.CompetitiveEventDraftContent?.MinimumAge ?? 0,
           MaximumAge = draft.CompetitiveEventDraftContent?.MaximumAge,
           Price = draft.CompetitiveEventDraftContent?.Price,
           CompetitiveSelection = draft.CompetitiveEventDraftContent?.CompetitiveSelection,
           Contacts = draft.CompetitiveEventDraftContent?.Contacts.Any() == true
            ? draft.CompetitiveEventDraftContent.Contacts.ToDto()
            : draft.CompetitiveEvent?.Contacts.ToDto() ?? new List<ContactsDto>(),
           CoverImageId = draft.CoverImageId,
           CoverageId = draft.CoverageId,
           CompetitiveEventAccountingTypeId = draft.CompetitiveEventAccountingTypeId,
           ImageIds = draft.Images?.Select(x => x.ExternalStorageId).ToList() ?? new List<string>(),
           SubDirectionIds = draft.CompetitiveEventDraftContent?.SubDirectionIds ?? [],
           CompetitiveEventDescriptionItems = draft.CompetitiveEventDraftContent?.CompetitiveEventDescriptionItems.ToDto()
       };
}