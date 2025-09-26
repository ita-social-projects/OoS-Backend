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
                 "The registration start time cannot be  equal to or earlier than the registration end time");
        }

        if (ScheduledStartTime >= ScheduledEndTime)
        {
            yield return new ValidationResult(
                 "The scheduled start time cannot be  equal to or earlier than the scheduled end time");
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