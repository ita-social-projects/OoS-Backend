using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.CompetitiveEvent;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventBaseDto : IValidatableObject, IHasContactsDto<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent>
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
    public CompetitiveEventStates State { get; set; } = CompetitiveEventStates.Published;

    [Required]
    public DateTimeOffset? RegistrationStartTime { get; set; }

    [Required]
    public DateTimeOffset? RegistrationEndTime { get; set; }

    public Guid? ParentId { get; set; }

    [Required]
    public int CoverageId { get; set; }

    [FromForm]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description item is required")]
    public List<CompetitiveEventDescriptionItemDto> CompetitiveEventDescriptionItems { get; set; }

    [Required]
    public DateTimeOffset ScheduledStartTime { get; set; }

    [Required]
    public DateTimeOffset ScheduledEndTime { get; set; }

    [Required]
    public uint NumberOfSeats { get; set; } = uint.MaxValue;

    [Required]
    public int CompetitiveEventAccountingTypeId { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.EnrollmentProcedureDescription)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    [Required]
    public Guid OrganizerOfTheEventId { get; set; }

    [Required(ErrorMessage = "Planned format of classes is required")]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning? PlannedFormatOfClasses { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.MaxVenueNameLength)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string VenueName { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.MaxTermsOfParticipationLength)]
    [RequiredIf(nameof(CompetitiveSelection), true, ErrorMessage = "Terms of participation is required")]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string TermsOfParticipation { get; set; }

    public bool? AreThereBenefits { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.MaxBenefitsLength)]
    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "Benefits is required")]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string Benefits { get; set; }

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Required]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int? MaximumAge { get; set; }

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public int? Price { get; set; }

    public bool? CompetitiveSelection { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<ContactsDto> Contacts { get; set; }

    [FromForm]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> SubDirectionIds { get; set; } = [];

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RegistrationStartTime >= RegistrationEndTime)
        {
            yield return new ValidationResult(
                 "The registration start time cannot be equal to or earlier than the registration end time");
        }

        if (ScheduledStartTime >= ScheduledEndTime)
        {
            yield return new ValidationResult(
                 "The scheduled start time cannot be equal to or earlier than the scheduled end time");
        }

        if (ScheduledStartTime <= RegistrationEndTime)
        {
            yield return new ValidationResult(
                 "The scheduled start time cannot be equal to or earlier than the registration end time");
        }

        if (NumberOfSeats != uint.MaxValue && (NumberOfSeats < 1 || NumberOfSeats > 100000))
        {
            yield return new ValidationResult("NumberOfSeats field should be in the range from 1 to 100000.", new[] { nameof(NumberOfSeats) });
        }

        if (MinimumAge >= MaximumAge)
        {
            yield return new ValidationResult("Minimum age should be less than Maximum age", new[] { nameof(MinimumAge), nameof(MaximumAge) });
        }
    }
}

public static class CompetitiveEventBaseDtoExtensions
{
    /// <summary>
    /// Converts a CompetitiveEventBaseDto into a new CompetitiveEvent domain model.
    /// </summary>
    /// <remarks>
    /// Maps DTO properties to a new OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent instance,
    /// applying sensible defaults where the DTO uses nullable properties:
    /// - RegistrationStartTime/RegistrationEndTime and PlannedFormatOfClasses default to their type default when null.
    /// - AreThereBenefits and CompetitiveSelection default to false when null.
    /// - MaximumAge and Price default to 0 when null.
    /// Contacts are converted via Contacts.ToModel(); SubDirectionIds are projected into SubDirection entries.
    /// Note: fields such as AdditionalDescription and PreferentialTermsOfParticipation are not mapped from the DTO.
    /// </remarks>
    /// <returns>A newly created CompetitiveEvent populated from the DTO.</returns>
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent ToModel(this CompetitiveEventBaseDto dto)
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
        SubDirections = [.. (dto.SubDirectionIds ?? []).Select(id => new SubDirection { Id = id })],
    };

    /// <summary>
    /// Copies values from a CompetitiveEventBaseDto into an existing CompetitiveEvent model instance and returns that instance.
    /// </summary>
    /// <param name="dto">Source DTO providing updated values.</param>
    /// <param name="model">Target model to be updated (mutated in-place and returned).</param>
    /// <returns>The same <see cref="OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent"/> instance passed in via <paramref name="model"/> after applying updates.</returns>
    /// <remarks>
    /// Fields that retain the model's existing value when the DTO value is null: RegistrationStartTime, RegistrationEndTime, PlannedFormatOfClasses, AreThereBenefits, MaximumAge, Price, CompetitiveSelection, and Contacts (Contacts is mapped via <c>dto.Contacts?.ToModel()</c>).
    /// SubDirections is replaced with a new list built from <c>dto.SubDirectionIds</c>
    /// </remarks>
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent SetToModel(this CompetitiveEventBaseDto dto, OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
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
        model.SubDirections = [.. (dto.SubDirectionIds ?? []).Select(id => new SubDirection { Id = id })];

        return model;
    }
}