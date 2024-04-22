using OutOfSchool.Licenses.Models;
using System.Text;

namespace OutOfSchool.Licenses.Services;

public class PackagesInfoToPlainTextConverter : IPackagesConverter
{
    public string Convert(Dictionary<string, IEnumerable<PackageWithLicense>> data)
    {
        Console.WriteLine($"Converting the package info to text format started.");

        if (data is null)
        {
            return string.Empty;
        }

        StringBuilder textData = new();

        foreach (var project in data.Keys)
        {
            textData.AppendLine($"{Constants.ProjectProperty}: {project} ({data[project].Count()})");

            foreach (var package in data[project])
            {
                textData.AppendLine();
                textData.AppendLine($"{Constants.PackageProperty}: {package.Name}");
                textData.AppendLine($"{Constants.VersionProperty}: {package.Version}");
                textData.AppendLine($"{Constants.LicenseProperty}: {package.License}");
                textData.AppendLine($"{Constants.LicenseUrlProperty}: {package.LicenseUrl}");
            }

            textData.AppendLine();
        }

        Console.WriteLine($"Converting the package info to text format finished.");

        return textData.ToString();
    }
}
