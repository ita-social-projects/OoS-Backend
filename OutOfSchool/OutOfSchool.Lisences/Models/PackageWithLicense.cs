namespace OutOfSchool.Licenses.Models;
public class PackageWithLicense : PackageBase
{
    public string License { get; set; } = string.Empty;

    public string LicenseUrl { get; set; } = string.Empty;
}
