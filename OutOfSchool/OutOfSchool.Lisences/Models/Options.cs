using OutOfSchool.Licenses.Util;

namespace OutOfSchool.Licenses.Models;

public class Options
{
    private string solutionDirectory = string.Empty;
    
    private string outputFileName = string.Empty;

    public string SolutionDirectory
    {
        get => solutionDirectory;
        
        set
        {
            if (Validators.IsDirectoryNameValid(value))
            {
                solutionDirectory = Path.GetFullPath(value);
            }
            else
            {
                throw new ArgumentException("The solution directory name isn't valid.", nameof(value));
            }
        }
    }

    public string OutputFileName
    {
        get => outputFileName;
        
        set
        {
            if (Validators.IsFileNameValid(value))
            {
                outputFileName = Path.GetFullPath(value);
            }
            else
            {
                throw new ArgumentException("The output file name isn't valid.", nameof(value));
            }
        }

    }
}
