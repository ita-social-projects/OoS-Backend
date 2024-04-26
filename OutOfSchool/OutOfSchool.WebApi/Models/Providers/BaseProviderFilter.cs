namespace OutOfSchool.WebApi.Models.Providers;

public class BaseProviderFilter : SearchStringFilter
{
    public Guid InstitutionId { get; set; } = Guid.Empty;

    public long CATOTTGId { get; set; } = 0;

    //public IReadOnlyCollection<ProviderLicenseStatus> LicenseStatus { get; set; } = new List<ProviderLicenseStatus>();
}