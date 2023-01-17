namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopBySettlementsFilter : WorkshopFilter
{
    public IEnumerable<long> SettlementsIds { get; set; } = new List<long>();
}
