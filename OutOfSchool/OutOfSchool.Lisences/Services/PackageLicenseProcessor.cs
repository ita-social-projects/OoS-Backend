using OutOfSchool.Licenses.Models;
using OutOfSchool.Licenses.Util;
using System.Xml.Linq;

namespace OutOfSchool.Licenses.Services;
public class PackageLicenseProcessor : IProcessor<PackageWithLicense>
{
    private readonly PackageBase package;

    public PackageLicenseProcessor(PackageBase package)
    {
        ArgumentNullException.ThrowIfNull(package);

        this.package = package;
    }
    public PackageWithLicense Process()
    {
        var nuspecFileName = FileHelper.GetNuSpecFileName(package.Name.ToLowerInvariant(), package.Version);

        var license = GetLicense(nuspecFileName);

        return new PackageWithLicense
        {
            Name = package.Name,
            Version = package.Version,
            License = license.license,
            LicenseUrl = license.licenseUrl
        };
    }

    private (string license, string licenseUrl) GetLicense(string fileName)
    {
        Console.WriteLine($"Getting the package license info from nuspec file {fileName} started.");

        if (!File.Exists(fileName))
        {
            return (string.Empty, string.Empty);
        }

        using var textReader = new StreamReader(fileName);

        var document = XDocument.Load(textReader);

        var licenseElement = document.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals(Constants.LicenseProperty));
        var licenseUrlElement = document.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals(Constants.LicenseUrlProperty));

        Console.WriteLine($"Getting the package license info from nuspec file finished. License: {licenseElement?.Value}, version: {licenseUrlElement?.Value}.");

        return (licenseElement?.Value ?? string.Empty, licenseUrlElement?.Value ?? string.Empty);
    }
}
