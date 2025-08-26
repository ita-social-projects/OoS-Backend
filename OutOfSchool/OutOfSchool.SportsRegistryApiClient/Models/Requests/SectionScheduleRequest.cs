using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SectionScheduleRequest : IValidatableObject
{
    [Required(ErrorMessage = "sectionScheduleWeekday is required.")]
    public Weekday SectionScheduleWeekday { get; set; }

    [Required(ErrorMessage = "sectionScheduleTimeFrom is required.")]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$", ErrorMessage = "sectionScheduleTimeFrom must be in HH:mm:ss format.")]
    public string SectionScheduleTimeFrom { get; set; } = null!; // e.g. 09:00:00

    [Required(ErrorMessage = "sectionScheduleTimeTo is required.")]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$", ErrorMessage = "sectionScheduleTimeTo must be in HH:mm:ss format.")]
    public string SectionScheduleTimeTo { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // time vaidation
        if (TimeSpan.TryParse(SectionScheduleTimeFrom, out var timeFrom) &&
            TimeSpan.TryParse(SectionScheduleTimeTo, out var timeTo))
        {
            if (timeFrom >= timeTo)
            {
                yield return new ValidationResult(
                    $"SectionScheduleTimeFrom '{SectionScheduleTimeFrom}' must be before SectionScheduleTimeTo '{SectionScheduleTimeTo}' in schedule entry.",
                    new[] { nameof(SectionScheduleTimeFrom), nameof(SectionScheduleTimeTo) });
            }
        }
        else 
        {
            yield return new ValidationResult(
                     $"Invalid time format for SectionScheduleTimeFrom '{SectionScheduleTimeFrom}' or SectionScheduleTimeTo '{SectionScheduleTimeTo}'. Expected format is HH:mm:ss.",
                     new[] { nameof(SectionScheduleTimeFrom), nameof(SectionScheduleTimeTo) });
        }
    }
}