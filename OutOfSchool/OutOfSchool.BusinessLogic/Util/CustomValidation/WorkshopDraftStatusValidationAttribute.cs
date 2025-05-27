using OutOfSchool.Services.Enums.WorkshopStatus;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class WorkshopDraftStatusValidationAttribute : ValidationAttribute
{
    private static readonly HashSet<WorkshopDraftStatus> AllowedStatuses = new()
    {
        WorkshopDraftStatus.PendingModeration,
        WorkshopDraftStatus.EditedByModerator,
        WorkshopDraftStatus.Rejected
    };

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is HashSet<WorkshopDraftStatus> statuses)
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
