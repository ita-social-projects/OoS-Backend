using Microsoft.Extensions.Configuration;
using OutOfSchool.Licenses.Models;
using OutOfSchool.Licenses.Services;

namespace OutOfSchool.Licenses;

public class Program
{
    static void Main(string[] args)
    {
        Options options = args.Length == 2
            ? GetOptionsFromCommandLineArguments(args)
            : GetOptionsFromAppSettingsFile();

        Execute(options);
    }

    private static void Execute(Options options)
    {
        var projects = GetSolutionProjects(options.SolutionDirectory);

        if (!projects.Any())
        {
            Console.WriteLine("The solution directory doesn't contain any projects.");
            return;
        }

        var projectsPackagesInfo = GetProjectsPackagesInfo(projects);

        var projectsPackagesInfoWithLicenses = GetProjectsPackagesInfoWithLicenses(projectsPackagesInfo);

        SaveToFile(options.OutputFileName, projectsPackagesInfoWithLicenses);
    }

    private static Options GetOptionsFromCommandLineArguments(string[] args) =>
        new Options
        {
            SolutionDirectory = args[0],
            OutputFileName = args[1],
        };

    private static Options GetOptionsFromAppSettingsFile() =>
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build()
            .GetSection(nameof(Options))
            .Get<Options>();

    private static IEnumerable<string> GetSolutionProjects(string solutionDirectory)
    {
        Console.WriteLine($"Getting the projects from solution started.");

        var solutionProcessor = new SolutionProcessor(solutionDirectory);
        var projects = solutionProcessor.Process();

        Console.WriteLine($"Getting the projects from solution finished.");

        return projects;
    }

    private static Dictionary<string, IEnumerable<PackageBase>> GetProjectsPackagesInfo(IEnumerable<string> projects)
    {
        Console.WriteLine($"Getting the projects packages info from projects started.");

        var projectsPackagesInfo = new Dictionary<string, IEnumerable<PackageBase>>();

        foreach (var project in projects)
        {
            var projectPackagesInfoProcessor = new ProjectPackagesInfoProcessor(project);
            projectsPackagesInfo.Add(project, projectPackagesInfoProcessor.Process());
        }

        Console.WriteLine($"Getting the projects packages info from projects finished.");

        return projectsPackagesInfo;
    }

    private static Dictionary<string, IEnumerable<PackageWithLicense>> GetProjectsPackagesInfoWithLicenses(Dictionary<string, IEnumerable<PackageBase>> projectsPackagesInfo)
    {
        Console.WriteLine($"Getting the projects packages licenses started.");

        var projectsPackagesInfoWithLicenses = new Dictionary<string, IEnumerable<PackageWithLicense>>();

        foreach (var project in projectsPackagesInfo)
        {
            var packagesWithLicenses = new List<PackageWithLicense>();

            foreach (var package in project.Value)
            {
                var packageLicenseProcessor = new PackageLicenseProcessor(package);
                packagesWithLicenses.Add(packageLicenseProcessor.Process());
            }

            projectsPackagesInfoWithLicenses.Add(project.Key, packagesWithLicenses);
        }

        Console.WriteLine($"Getting the projects packages licenses finished.");

        return projectsPackagesInfoWithLicenses;
    }

    private static void SaveToFile(string fileName, Dictionary<string, IEnumerable<PackageWithLicense>> data)
    {
        Console.WriteLine($"Saving to file {fileName} started.");

        var textFile = new TextFileWriter(fileName, new PackagesInfoToPlainTextConverter());
        textFile.Write(data);
        
        Console.WriteLine($"Saving to file {fileName} finished.");
    }
}
