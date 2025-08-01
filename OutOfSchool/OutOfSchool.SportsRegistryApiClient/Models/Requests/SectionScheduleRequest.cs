using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SectionScheduleRequest
{
    [Required(ErrorMessage = "sectionScheduleWeekday is required.")]
    public string SectionScheduleWeekday { get; set; } = null!; // e.g. MONDAY

    [Required(ErrorMessage = "sectionScheduleTimeFrom is required.")]
    public string SectionScheduleTimeFrom { get; set; } = null!; // e.g. 09:00:00

    [Required(ErrorMessage = "sectionScheduleTimeTo is required.")]
    public string SectionScheduleTimeTo { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // weekday validation
        if (!Enum.TryParse(typeof(Weekday), SectionScheduleWeekday, ignoreCase: true, out _))
        {
            yield return new ValidationResult(
                $"Invalid weekday for : '{SectionScheduleWeekday}'. Must be one of: {string.Join(", ", Enum.GetNames(typeof(Weekday)))}",
                new[] { nameof(SectionScheduleWeekday) });
        }

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