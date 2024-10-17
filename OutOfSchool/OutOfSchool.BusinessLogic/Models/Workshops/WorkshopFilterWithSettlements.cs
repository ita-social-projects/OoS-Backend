namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopFilterWithSettlements : WorkshopFilter
{
    public IEnumerable<long> SettlementsIds { get; set; } = new List<long>();
}
