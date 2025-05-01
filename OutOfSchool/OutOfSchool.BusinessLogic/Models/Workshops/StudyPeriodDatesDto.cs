using OutOfSchool.Services.Models.WorkshopDrafts;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class StudyPeriodDatesDto
{
    [Required(ErrorMessage = "Study period start date is required")]
    public DateOnly StartDate { get; set; }


    [Required(ErrorMessage = "Study period end date is required")]
    public DateOnly EndDate { get; set; }
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
