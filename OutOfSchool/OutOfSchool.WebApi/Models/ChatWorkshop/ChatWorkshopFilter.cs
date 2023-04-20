namespace OutOfSchool.WebApi.Models.ChatWorkshop;

public class ChatWorkshopFilter: OffsetFilter
{
    public IEnumerable<Guid> WorkshopIds { get; set; } = new List<Guid>();

    public string SearchText { get; set; } = string.Empty;
}
