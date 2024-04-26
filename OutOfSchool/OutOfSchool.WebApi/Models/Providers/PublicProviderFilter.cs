using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Providers;

public class PublicProviderFilter : BaseProviderFilter
{
    public IReadOnlyCollection<ProviderStatus> Status { get; set; } = new List<ProviderStatus>();
}
