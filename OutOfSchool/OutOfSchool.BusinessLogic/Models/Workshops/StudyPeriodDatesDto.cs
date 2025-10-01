using OutOfSchool.Services.Models.WorkshopDrafts;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class StudyPeriodDatesDto : IValidatableObject
{
    [Required(ErrorMessage = "Study period start date is required")]
    public DateOnly StartDate { get; set; }


    [Required(ErrorMessage = "Study period end date is required")]
    public DateOnly EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate == default)
            yield return new ValidationResult("Study period start date is required. The data type in the 'StartDate' field must be in the format YYYY-MM-DD.",
                new[] { nameof(StartDate) });

        if (EndDate == default)
            yield return new ValidationResult("Study period end date is required. The data type in the 'EndDate' field must be in the format YYYY-MM-DD.",
                new[] { nameof(EndDate) });

        if (StartDate != default && EndDate != default && StartDate > EndDate)
            yield return new ValidationResult("The end date cannot be earlier than the start date.", new[] { nameof(StartDate), nameof(EndDate) });
    }
}

public static class StudyPeriodDatesDtoExtensions
{
    public static StudyPeriodDatesDto ToStudyPeriodDatesDto(this WorkshopDraftContent draft)
    => new()
    {
        StartDate = draft.StudyPeriodStartDate,
        EndDate = draft.StudyPeriodEndDate,
    };

    public static StudyPeriodDatesDto ToStudyPeriodDatesDto(this Workshop model)
    => new()
    {
        StartDate = model.StudyPeriodStartDate,
        EndDate = model.StudyPeriodEndDate,
    };
}
