using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static OutOfSchool.BusinessLogic.Validators.ConditionalValidationAttributes;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;

[JsonDerivedType(typeof(CompetitiveEventAboutDto), typeDiscriminator: "withAboutProperties")]
[JsonDerivedType(typeof(CompetitiveEventDescriptionDto), typeDiscriminator: "withDescription")]
[JsonDerivedType(typeof(CompetitiveEventContactsDto), typeDiscriminator: "withContacts")]
public class CompetitiveEventAboutDto : IValidatableObject
{
    // This property uses only for storing dto in Redis
    [ConditionalRequired("Images", ErrorMessage = "The cover image is required")]
    public string Base64CoverImage { get; set; }

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

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Required]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int? MaximumAge { get; set; }

    [Required]
    public DateTimeOffset ScheduledStartTime { get; set; }

    [Required]
    public DateTimeOffset ScheduledEndTime { get; set; }

    [Required]
    public DateTimeOffset? RegistrationStartTime { get; set; }

    [Required]
    public DateTimeOffset? RegistrationEndTime { get; set; }

    [Required]
    public int CompetitiveEventAccountingTypeId { get; set; }

    [Required]
    public uint NumberOfSeats { get; set; } = uint.MaxValue;

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
