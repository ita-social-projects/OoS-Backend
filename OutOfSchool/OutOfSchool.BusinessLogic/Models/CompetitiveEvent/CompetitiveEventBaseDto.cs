using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using static OutOfSchool.BusinessLogic.Validators.RequiredIfMinAndMaxLengthAttributes;
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventBaseDto : IValidatableObject, IHasContactsDto<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent>
{
    public Guid Id { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxCompetitiveEventTitleLength)]
    [MinLength(Constants.MinCompetitiveEventTitleLength)]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Title must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string Title { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(Constants.MaxCompetitiveEventShortTitleLength)]
    [MinLength(Constants.MinCompetitiveEventShortTitleLength)]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Short title must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
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
    public List<CompetitiveEventDescriptionItemDto> CompetitiveEventDescriptionItems { get; set; } = [];

    [Required]
    public DateTimeOffset ScheduledStartTime { get; set; }

    [Required]
    public DateTimeOffset ScheduledEndTime { get; set; }

    [Required]
    public uint NumberOfSeats { get; set; } = uint.MaxValue;

    [Required]
    public int CompetitiveEventAccountingTypeId { get; set; }

    [Required]
    [MinLength(Constants.MinLengthOfDescriptionOfTheEnrollmentProcedureForCompetitiveEvent)]
    [MaxLength(Constants.MaxLengthOfDescriptionOfTheEnrollmentProcedureForCompetitiveEvent)]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "DescriptionOfTheEnrollmentProcedure field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    [Required]
    public Guid OrganizerOfTheEventId { get; set; }

    [Required]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning? PlannedFormatOfClasses { get; set; }

    [MinLength(Constants.MinVenueNameLength)]
    [MaxLength(Constants.MaxVenueNameLength)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string VenueName { get; set; }

    [RequiredIf(nameof(CompetitiveSelection), true, ErrorMessage = "Competitive selection description is required")]
    [RequiredIfMinLength(nameof(CompetitiveSelection), true, Constants.MinCompetitiveSelectionDescriptionLength, ErrorMessage = "Competitive selection description must contain at least 3 letters.")]
    [RequiredIfMaxLength(nameof(CompetitiveSelection), true, Constants.MaxCompetitiveSelectionDescriptionLength, ErrorMessage = "Competitive selection description must not contain greater than 2000 letters.")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Competitive selection description must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string CompetitiveSelectionDescription { get; set; }

    public bool? AreThereBenefits { get; set; }

    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "Benefits is required")]
    [RequiredIfMinLength(nameof(AreThereBenefits), true, Constants.MinBenefitsLength, ErrorMessage = "Benefits must contain at least 3 letters.")]
    [RequiredIfMaxLength(nameof(AreThereBenefits), true, Constants.MaxBenefitsLength, ErrorMessage = "Benefits must not contain greater than 2000 letters.")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Benefits field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string Benefits { get; set; }

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Required]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int? MaximumAge { get; set; }

    public bool IsPaid { get; set; } = false;

    [MaxDecimalPlaces(2, ErrorMessage = "Price field must have maximum two decimal places.")]
    [RequiredIf(nameof(IsPaid), true, ErrorMessage = "Price is required")]
    [ModelBinder(BinderType = typeof(DecimalDotModelBinder))]
    [JsonConverter(typeof(DecimalDotJsonConverter))]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 0 to 100 000")]
    public decimal? Price { get; set; } = default;

    public bool? CompetitiveSelection { get; set; }

    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitle { get; set; } = string.Empty;

    [MaxLength(Constants.MaxProviderFullTitleLength)]
    public string ProviderTitleEn { get; set; } = string.Empty;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one contact is required")]
    public List<ContactsDto> Contacts { get; set; } = [];

    [FromForm]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> SubDirectionIds { get; set; } = [];

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RegistrationStartTime > RegistrationEndTime)
        {
            yield return new ValidationResult(
                 "Registration start time must be before registration end time");
        }

        if (ScheduledStartTime >= ScheduledEndTime)
        {
            yield return new ValidationResult(
                 "Scheduled start time must be before scheduled end time");
        }

        if (ScheduledStartTime < RegistrationEndTime)
        {
            yield return new ValidationResult(
                 "Scheduled start time must be after registration end time");
        }

        if (NumberOfSeats != uint.MaxValue && (NumberOfSeats < 1 || NumberOfSeats > 100000))
        {
            yield return new ValidationResult("NumberOfSeats field should be in the range from 1 to 100000.", [nameof(NumberOfSeats)]);
        }

        if (MinimumAge >= MaximumAge)
        {
            yield return new ValidationResult("Minimum age should be less than Maximum age", [nameof(MinimumAge), nameof(MaximumAge)]);
        }

        // validate Price when IsPaid is true
        if (IsPaid && (!Price.HasValue || Price < 0.01M))
        {
            yield return new ValidationResult("Price must be specified and must be in the range from 0.01 to 100000.00 when the competitive event is paid.", [nameof(Price)]);
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
        CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
        AreThereBenefits = dto.AreThereBenefits ?? false,
        Benefits = dto.Benefits,
        MinimumAge = dto.MinimumAge,
        MaximumAge = dto.MaximumAge ?? 0,
        IsPaid = dto.IsPaid,
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
        model.CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription;
        model.AreThereBenefits = dto.AreThereBenefits ?? model.AreThereBenefits;
        model.Benefits = dto.Benefits;
        model.MinimumAge = dto.MinimumAge;
        model.MaximumAge = dto.MaximumAge ?? model.MaximumAge;
        model.IsPaid = dto.IsPaid;
        model.Price = dto.Price ?? model.Price;
        model.CompetitiveSelection = dto.CompetitiveSelection ?? model.CompetitiveSelection;
        model.Contacts = dto.Contacts?.ToModel() ?? model.Contacts;
        model.SubDirections = [.. (dto.SubDirectionIds ?? []).Select(id => new SubDirection { Id = id })];

        return model;
    }
}