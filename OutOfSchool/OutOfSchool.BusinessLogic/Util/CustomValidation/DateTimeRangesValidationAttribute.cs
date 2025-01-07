using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.Services.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;
public class DateTimeRangeValidator
{
    public  IEnumerable<ValidationResult> Validate(DateTimeRangeDraftDto range)
    {
        if (range.StartTime > range.EndTime)
        {
            yield return new ValidationResult($"Invalid date range: StartTime '{range.StartTime}' is later than EndTime '{range.EndTime}'.");
        }

        if (range.StartTime.CompareTo(range.EndTime) == 0)
        {
            yield return new ValidationResult($"Invalid date range: StartTime '{range.StartTime}' is equal to EndTime '{range.EndTime}'.");
        }

        if (range.Workdays == null || !range.Workdays.Any() || range.Workdays.Contains(DaysBitMask.None))
        {
            yield return new ValidationResult("Workdays are required and cannot contain 'None'.");
        }
    }
}