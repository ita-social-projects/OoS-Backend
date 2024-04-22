using OutOfSchool.Licenses.Models;
using OutOfSchool.Licenses.Util;
using System.Text.Json;

namespace OutOfSchool.Licenses.Services;

public class ProjectPackagesInfoProcessor : IProcessor<IEnumerable<PackageBase>>
{
    private readonly string project;

    public ProjectPackagesInfoProcessor(string project)
    {
        ArgumentNullException.ThrowIfNull(nameof(project));

        this.project = project;
    }

    public IEnumerable<PackageBase> Process() =>
        GetPackages(project);

    private IEnumerable<PackageBase> GetPackages(string project) =>
        GetPackagesFromAssetsFile(FileHelper.GetAssetsFileName(project));

    private IEnumerable<PackageBase> GetPackagesFromAssetsFile(string fileName)
    {
        Console.WriteLine($"Getting the package info from assets file {fileName} started.");

        if (!File.Exists(fileName))
        {
            return Enumerable.Empty<PackageBase>();
        }

        using var fileStream = File.OpenRead(fileName);
        var document = JsonDocument.Parse(fileStream);

        var packages = new List<PackageBase>();

        if (document.RootElement.TryGetProperty(Constants.TargetsProperty, out var targets))
        {
            foreach (var target in targets.EnumerateObject())
            {
                foreach (var element in target.Value.EnumerateObject())
                {
                    var nameVersion = element.Name.Split(Constants.PackageNameVersionDivider);

                    if (nameVersion.Length == 2
                        && element.Value.TryGetProperty(Constants.TypeProperty, out var type)
                        && type.ValueEquals(Constants.PackageProperty))
                    {
                        packages.Add(new PackageBase
                        {
                            Name = nameVersion[0],
                            Version = nameVersion[1]
                        });
                    }
                }
            }
        }

        Console.WriteLine($"Getting the package info from assets file finished.");

        return packages;
    }
}
