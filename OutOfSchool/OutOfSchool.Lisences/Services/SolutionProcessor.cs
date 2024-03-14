namespace OutOfSchool.Licenses.Services;

public class SolutionProcessor : IProcessor<IEnumerable<string>>
{
    private readonly string solutionDirectory;

    public SolutionProcessor(string solutionDirectory)
    {
        ArgumentNullException.ThrowIfNull(nameof(solutionDirectory));

        this.solutionDirectory = solutionDirectory;
    }

    public IEnumerable<string> Process()
    {
        return GetProjects(solutionDirectory);
    }

    private IEnumerable<string> GetProjects(string solutionDirectory) =>
        Directory.EnumerateFiles(solutionDirectory, Constants.ProjectPattern, SearchOption.AllDirectories);
}
