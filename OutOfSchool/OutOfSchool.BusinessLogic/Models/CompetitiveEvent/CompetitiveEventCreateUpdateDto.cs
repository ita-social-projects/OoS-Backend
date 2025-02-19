using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventCreateUpdateDto : CompetitiveEventBaseDto, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ScheduledEndTime <= ScheduledStartTime)
        {
            yield return new ValidationResult("Scheduled end time must be after start time.");
        }
        
        if (RegistrationEndTime <= RegistrationStartTime)
        {
            yield return new ValidationResult("Registration end time must be after start time.");
        }
    }
}