namespace OutOfSchool.BusinessLogic.Models.Workshops;
public class WorkshopCreateUpdateDto : WorkshopBaseDto
{
    public List<long> TagIds { get; set; } = new List<long>();
}
