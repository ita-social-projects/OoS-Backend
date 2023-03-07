using OutOfSchool.Common.Enums;

namespace OutOfSchool.ElasticsearchData.Models;
public class WorkshopProviderStatusES : IPartial<WorkshopES>
{
    public ProviderStatus ProviderStatus { get; set; }
}
