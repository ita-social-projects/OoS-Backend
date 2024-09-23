namespace OutOfSchool.BusinessLogic.Models.Workshops;
public class WorkshopCreate : WorkshopBaseDto
{
    public List<long> TagIds { get; set; }
}
