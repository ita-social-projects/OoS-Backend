using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopBaseDto : WorkshopWithTeachersDto, IValidatableObject
{
    public Guid Id { get; set; }
}
