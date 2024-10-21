namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopTagsUpdateDto
{
    public Guid WorkshopId { get; set; }

    public List<long> TagIds { get; set; }
}
