using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class StudyPeriodDatesDto
{
    [Required(ErrorMessage = "Study period start date is required")]
    public DateOnly StartDate { get; set; }


    [Required(ErrorMessage = "Study period end date is required")]
    public DateOnly EndDate { get; set; }
}
