using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
public class CompetitiveEventV2Dto : CompetitiveEventDto, IHasCoverImage, IHasImages
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<IFormFile> ImageFiles { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from CompetitiveEventDto
        foreach (var error in base.Validate(validationContext))
            yield return error;

        if ((CoverImage is null) == string.IsNullOrEmpty(CoverImageId))
        {
            yield return new ValidationResult(
                "Either CoverImage or CoverImageId must be filled in, but not both.",
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

public static class CompetitiveEventV2DtoExtensions
{
    /// <summary>
    /// Maps a domain <see cref="OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent"/> to a <see cref="CompetitiveEventV2Dto"/>.
    /// </summary>
    /// <remarks>
    /// Preserves core fields (identifiers, titles, times, pricing, limits, contacts, coverage, etc.), converts description items and contacts via their respective <c>ToDto()</c> mappers,
    /// and projects image metadata: <see cref="CompetitiveEventV2Dto.CoverImageId"/> is copied from the model and <see cref="CompetitiveEventV2Dto.ImageIds"/> is populated from each image's <c>ExternalStorageId</c>.
    /// SubDirections marked as deleted are excluded; when no subdirections or description/contacts are present, the corresponding DTO collections are empty or null according to the mapper behavior.
    /// </remarks>
    /// <returns>A new <see cref="CompetitiveEventV2Dto"/> populated from the source model.</returns>
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
        ScheduledStartTime = model.ScheduledStartTime,
        ScheduledEndTime = model.ScheduledEndTime,
        NumberOfSeats = model.NumberOfSeats,
        CompetitiveEventAccountingTypeId = model.CompetitiveEventAccountingTypeId,
        DescriptionOfTheEnrollmentProcedure = model.DescriptionOfTheEnrollmentProcedure,
        OrganizerOfTheEventId = model.OrganizerOfTheEventId,
        PlannedFormatOfClasses = model.PlannedFormatOfClasses,
        VenueName = model.VenueName,
        CompetitiveSelectionDescription = model.CompetitiveSelectionDescription,
        AreThereBenefits = model.AreThereBenefits,
        Benefits = model.Benefits,
        MinimumAge = model.MinimumAge,
        MaximumAge = model.MaximumAge,
        Price = model.Price,
        CompetitiveSelection = model.CompetitiveSelection,
        Contacts = model.Contacts?.ToDto(),
        SubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(s => s.Id).ToList() ?? [],
        Coverage = model.Coverage?.ToDto(),
        DirectionSubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(
            s => new DirectionSubDirectionIdsDto
            {
                DirectionId = s.DirectionId,
                SubDirectionId = s.Id
            })
        .ToList() ?? [],
        CoverImageId = model.CoverImageId,
        ImageIds = model.Images?.Select(i => i.ExternalStorageId).ToList() ?? [],
    };

    /// <summary>
    /// Converts a sequence of domain CompetitiveEvent models into a list of CompetitiveEventV2Dto.
    /// </summary>
    /// <returns>A List of CompetitiveEventV2Dto produced by mapping each input model with <see cref="ToV2Dto(OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent)"/>.</returns>
    public static List<CompetitiveEventV2Dto> ToV2Dto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
    => list.MapToList(ToV2Dto);

    /// <summary>
    /// Converts a CompetitiveEventDraft into a CompetitiveEventV2Dto by mapping draft content and related metadata.
    /// </summary>
    /// <remarks>
    /// Maps values from <c>draft.CompetitiveEventDraftContent</c> when present and preserves draft-level metadata such as <c>CoverImageId</c>, <c>Images</c>, <c>CoverageId</c>, and <c>CompetitiveEventAccountingTypeId</c>. Behaviors for missing values:
    /// - If <c>CompetitiveEventId</c> is null the DTO <c>Id</c> will be <c>Guid.Empty</c>.
    /// - Numeric and DateTime fields fall back to their default values when null (e.g., <c>MinimumAge</c> defaults to 0).
    /// - <c>Contacts</c>, <c>ImageIds</c>, and other collections default to empty lists when absent.
    /// - <c>SubDirectionIds</c> are taken from draft content if available; otherwise they are taken from <c>draft.CompetitiveEvent.SubDirections</c> when present.
    /// </remarks>
    /// <returns>A CompetitiveEventV2Dto populated from the provided draft.</returns>
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
            ScheduledStartTime = draft.CompetitiveEventDraftContent?.ScheduledStartTime ?? default,
            ScheduledEndTime = draft.CompetitiveEventDraftContent?.ScheduledEndTime ?? default,
            NumberOfSeats = draft.CompetitiveEventDraftContent?.NumberOfSeats ?? default,
            DescriptionOfTheEnrollmentProcedure = draft.CompetitiveEventDraftContent?.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = draft.CompetitiveEventDraftContent.OrganizerOfTheEventId,
            PlannedFormatOfClasses = draft.CompetitiveEventDraftContent?.PlannedFormatOfClasses,
            VenueName = draft.CompetitiveEventDraftContent?.VenueName,
            CompetitiveSelectionDescription = draft.CompetitiveEventDraftContent?.CompetitiveSelectionDescription,
            AreThereBenefits = draft.CompetitiveEventDraftContent?.AreThereBenefits,
            Benefits = draft.CompetitiveEventDraftContent?.Benefits,
            MinimumAge = draft.CompetitiveEventDraftContent?.MinimumAge ?? 0,
            MaximumAge = draft.CompetitiveEventDraftContent?.MaximumAge,
            Price = draft.CompetitiveEventDraftContent?.Price,
            CompetitiveSelection = draft.CompetitiveEventDraftContent?.CompetitiveSelection,
            Contacts = draft.CompetitiveEventDraftContent?.Contacts?.ToDto() ?? [],
            CoverImageId = draft.CoverImageId,
            ImageIds = draft.Images?.Select(x => x.ExternalStorageId).ToList() ?? [],
            CoverageId = draft.CoverageId,
            CompetitiveEventAccountingTypeId = draft.CompetitiveEventAccountingTypeId,
            SubDirectionIds = draft.CompetitiveEventDraftContent?.SubDirectionIds ??
                              draft.CompetitiveEvent?.SubDirections?.Select(s => s.Id).ToList() ?? [],
            CompetitiveEventDescriptionItems = draft.CompetitiveEventDraftContent?.CompetitiveEventDescriptionItems?.ToDto()
        };
    }

    public static void SetToDraft(this CompetitiveEventV2Dto dto, OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft draft)
    {
        draft.ProviderId = dto.OrganizerOfTheEventId;
        draft.CompetitiveEventId = dto.Id == Guid.Empty ? (Guid?)null : dto.Id;
        draft.CompetitiveEventDraftContent = dto.ToDraftContent();
        // This is needed for search
        draft.CATOTTGId = dto.Contacts.SingleOrDefault(c => c.IsDefault)?.Address?.CATOTTGId ?? 0;
    }

    public static OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft ToDraft(this CompetitiveEventV2Dto competitiveEventV2Dto)
        => new()
        {
            ProviderId = competitiveEventV2Dto.OrganizerOfTheEventId,
            CompetitiveEventId = competitiveEventV2Dto.Id == Guid.Empty ? null : competitiveEventV2Dto.Id,
            CoverImageId = competitiveEventV2Dto.CoverImageId,
            CoverageId = competitiveEventV2Dto.CoverageId,
            CompetitiveEventAccountingTypeId = competitiveEventV2Dto.CompetitiveEventAccountingTypeId,
            CompetitiveEventDraftContent = competitiveEventV2Dto.ToDraftContent(),
            // This is needed for search
            CATOTTGId = competitiveEventV2Dto.Contacts.SingleOrDefault(c => c.IsDefault)?.Address?.CATOTTGId ?? 0,
        };

    public static List<OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft> ToDraft(this IEnumerable<CompetitiveEventV2Dto> list)
        => list.MapToList(ToDraft);

    /// <summary>
    /// Converts a CompetitiveEventV2Dto into a CompetitiveEventDraftContent instance.
    /// </summary>
    /// <param name="competitiveEventV2Dto">Source DTO to convert; its nullable fields are mapped with sensible defaults.</param>
    /// <returns>
    /// A new CompetitiveEventDraftContent populated from the DTO. Nullable boolean and numeric fields are replaced with their default values when null;
    /// <see cref="Contacts"/> is an empty list if DTO contacts are null; <see cref="SubDirectionIds"/> is an empty list if null; 
    /// <see cref="CompetitiveEventDescriptionItems"/> is mapped using the DTO's ToModel() when present.
    /// </returns>
    public static CompetitiveEventDraftContent ToDraftContent(this CompetitiveEventV2Dto competitiveEventV2Dto)
    => new()
    {
        AreThereBenefits = competitiveEventV2Dto.AreThereBenefits ?? default,
        Benefits = competitiveEventV2Dto.Benefits,
        CompetitiveSelection = competitiveEventV2Dto.CompetitiveSelection ?? default,
        Contacts = competitiveEventV2Dto.Contacts?.ToModel() ?? new List<Contacts>(),
        DescriptionOfTheEnrollmentProcedure = competitiveEventV2Dto.DescriptionOfTheEnrollmentProcedure,
        MaximumAge = competitiveEventV2Dto.MaximumAge ?? default,
        MinimumAge = competitiveEventV2Dto.MinimumAge,
        NumberOfSeats = competitiveEventV2Dto.NumberOfSeats,
        OrganizerOfTheEventId = competitiveEventV2Dto.OrganizerOfTheEventId,
        ParentId = competitiveEventV2Dto.ParentId,
        PlannedFormatOfClasses = competitiveEventV2Dto.PlannedFormatOfClasses ?? default,
        Price = competitiveEventV2Dto.Price ?? default,
        RegistrationEndTime = competitiveEventV2Dto.RegistrationEndTime ?? default,
        RegistrationStartTime = competitiveEventV2Dto.RegistrationStartTime ?? default,
        ScheduledEndTime = competitiveEventV2Dto.ScheduledEndTime,
        ScheduledStartTime = competitiveEventV2Dto.ScheduledStartTime,
        ShortTitle = competitiveEventV2Dto.ShortTitle,
        Title = competitiveEventV2Dto.Title,
        CompetitiveSelectionDescription = competitiveEventV2Dto.CompetitiveSelectionDescription,
        VenueName = competitiveEventV2Dto.VenueName,
        SubDirectionIds = competitiveEventV2Dto.SubDirectionIds ?? [],
        CompetitiveEventDescriptionItems = competitiveEventV2Dto.CompetitiveEventDescriptionItems?.ToModel(),
    };

    /// <summary>
    /// Copies values from a CompetitiveEventV2Dto into an existing CompetitiveEvent domain model, updating the model in-place.
    /// </summary>
    /// <param name="dto">Source DTO containing new values; nullable properties on the DTO will not overwrite existing model values.</param>
    /// <param name="model">The existing domain model to update.</param>
    /// <returns>The same CompetitiveEvent instance after applying updates.</returns>
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent SetToModel(this CompetitiveEventV2Dto dto, OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
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
        model.CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription;
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
    /// Creates a new CompetitiveEvent domain model populated from the DTO.
    /// </summary>
    /// <remarks>
    /// Nullable DTO fields are converted with sensible defaults: null registration times and <see cref="PlannedFormatOfClasses"/> are set to their default values; <see cref="AreThereBenefits"/>, <see cref="CompetitiveSelection"/> default to false; <see cref="MaximumAge"/> and <see cref="Price"/> default to 0. Contacts are mapped via <c>dto.Contacts?.ToModel()</c>. The DTO's <see cref="CoverImageId"/> is copied to the model.
    /// </remarks>
    /// <returns>A new <see cref="OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent"/> instance with properties copied from the DTO.</returns>
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent ToModel(this CompetitiveEventV2Dto dto)
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
        CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
        AreThereBenefits = dto.AreThereBenefits ?? false,
        Benefits = dto.Benefits,
        MinimumAge = dto.MinimumAge,
        MaximumAge = dto.MaximumAge ?? 120,
        Price = dto.Price ?? 0,
        CompetitiveSelection = dto.CompetitiveSelection ?? false,
        Contacts = dto.Contacts?.ToModel(),
        CoverImageId = dto.CoverImageId,
    };
}
