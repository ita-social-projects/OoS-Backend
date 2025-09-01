using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SectionScheduleRequest : IValidatableObject
{
    [EnumDataType(typeof(Weekday), ErrorMessage = "sectionScheduleWeekday must be a valid enum value.")]
    public Weekday SectionScheduleWeekday { get; set; }

    [Required(ErrorMessage = "sectionScheduleTimeFrom is required.")]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$", ErrorMessage = "sectionScheduleTimeFrom must be in HH:mm:ss format.")]
    public string SectionScheduleTimeFrom { get; set; } = null!; // e.g. 09:00:00

    [Required(ErrorMessage = "sectionScheduleTimeTo is required.")]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$", ErrorMessage = "sectionScheduleTimeTo must be in HH:mm:ss format.")]
    public string SectionScheduleTimeTo { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // weekday validation
        if (!Enum.IsDefined(typeof(Weekday), SectionScheduleWeekday))
        {
            yield return new ValidationResult(
            "sectionScheduleWeekday must be specified.",
            new[] { nameof(SectionScheduleWeekday) });
        }

        // time vaidation
        var culture = CultureInfo.InvariantCulture;

        bool parsedFrom = TimeSpan.TryParse(SectionScheduleTimeFrom, culture, out var timeFrom);
        bool parsedTo = TimeSpan.TryParse(SectionScheduleTimeTo, culture, out var timeTo);

        if (!parsedFrom || !parsedTo)
        {
            yield return new ValidationResult(
                     $"Invalid time format for SectionScheduleTimeFrom '{SectionScheduleTimeFrom}' or SectionScheduleTimeTo '{SectionScheduleTimeTo}'. Expected format is HH:mm:ss.",
                     new[] { nameof(SectionScheduleTimeFrom), nameof(SectionScheduleTimeTo) });
            yield break; // No need to continue if parsing failed
        }
        if (timeFrom >= timeTo)
        {
            yield return new ValidationResult(
                $"SectionScheduleTimeFrom '{SectionScheduleTimeFrom}' must be before SectionScheduleTimeTo '{SectionScheduleTimeTo}' in schedule entry.",
                new[] { nameof(SectionScheduleTimeFrom), nameof(SectionScheduleTimeTo) });
        }
    }
}