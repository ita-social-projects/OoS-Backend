namespace OutOfSchool.BusinessLogic.Services.SearchString;

/// <summary>
/// Responsible for handling various operations related to search string processing.
/// </summary>
public interface ISearchStringService
{
    /// <summary>
    /// Splits the input string into words based on the current separators.
    /// </summary>
    /// <param name="input">The input string to be split.</param>
    /// <returns>An array of words split by the defined separators.</returns>
    string[] SplitSearchString(string input);
}