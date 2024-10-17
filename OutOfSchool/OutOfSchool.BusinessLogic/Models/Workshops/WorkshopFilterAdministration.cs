using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Workshops;
public class WorkshopFilterAdministration : SearchStringFilter
{
    public Guid InstitutionId { get; set; } = Guid.Empty;

    [Range(0, long.MaxValue, ErrorMessage = "Field value should be in a range from 0 to 9 223 372 036 854 775 807")]
    public long CATOTTGId { get; set; } = 0;

    //TODO add  WorkshopStatus after their approval
}
