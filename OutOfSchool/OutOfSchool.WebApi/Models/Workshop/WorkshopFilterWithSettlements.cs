namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopFilterWithSettlements : WorkshopFilter
{
    public IEnumerable<long> SettlementsIds { get; set; } = new List<long>();
}
