namespace OutOfSchool.Licenses.Util;
public static class FileHelper
{
    private const string outputTextFileName = "licenses.txt";

    private static string nugetRoot = Environment.GetEnvironmentVariable(Constants.NugetPackagesEnvironmentVariableName)
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        Constants.NugetDirectoryName,
                        Constants.PackagesDirectoryName);

    public static string GetOutputFileName(string solutionDirectory) =>
        string.IsNullOrWhiteSpace(solutionDirectory) || !Directory.Exists(solutionDirectory)
            ? Path.Combine(Directory.GetCurrentDirectory(), outputTextFileName)
            : Path.Combine(solutionDirectory, outputTextFileName);
    public static string GetAssetsFileName(string project) =>
        Path.Combine(Path.GetDirectoryName(project), Constants.ProjectObjDirectoryName, Constants.ProjectAssetsFileName);

    public static string GetNuSpecFileName(string packageName, string packageVersion)
        => Path.Combine(nugetRoot, packageName, packageVersion, $"{packageName}.{Constants.NuSpecFileExtension}");
}
