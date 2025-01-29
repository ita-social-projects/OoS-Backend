using OutOfSchool.Services.Enums.WorkshopStatus;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Util.CustomValidation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class WorkshopDraftStatusValidationAttribute : ValidationAttribute
{   
    private static readonly HashSet<WorkshopDraftStatus> AllowedStatuses = new()
    {
        WorkshopDraftStatus.PendingModeration,
        WorkshopDraftStatus.Rejected
    };

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is WorkshopDraftStatus status &&            
            AllowedStatuses.Contains(status))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            $"The status must be one of the following: {string.Join(", ", AllowedStatuses)}.");
    }
}
