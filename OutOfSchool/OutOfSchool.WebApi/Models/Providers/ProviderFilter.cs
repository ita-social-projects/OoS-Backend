using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderFilter : SearchStringFilter
{
    public Guid InstitutionId { get; set; } = Guid.Empty;

    public long CATOTTGId { get; set; } = 0;

    public IReadOnlyCollection<ProviderStatus> Status { get; set; } = new List<ProviderStatus>();

    public IReadOnlyCollection<ProviderLicenseStatus> LicenseStatus { get; set; } = new List<ProviderLicenseStatus>();
}