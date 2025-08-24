using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CompetitiveEventDraftStatusValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<CompetitiveEventDraftStatus> AllowedStatuses = new()
    {
        CompetitiveEventDraftStatus.PendingModeration,
        CompetitiveEventDraftStatus.EditedByModerator,
        CompetitiveEventDraftStatus.Rejected
    };

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is HashSet<CompetitiveEventDraftStatus> statuses)
        {
            var invalidStatuses = statuses.Except(AllowedStatuses).ToList();

            if (invalidStatuses.Any())
            {
                return new ValidationResult(
                    $"Invalid statuses: {string.Join(", ", invalidStatuses)}. " +
                    $"Allowed statuses are: {string.Join(", ", AllowedStatuses)}.");
            }

            return ValidationResult.Success;
        }

        return new ValidationResult("Invalid status collection type.");
    }
}
