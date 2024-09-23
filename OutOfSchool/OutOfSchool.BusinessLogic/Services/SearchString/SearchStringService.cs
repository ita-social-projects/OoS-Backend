using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config.SearchString;

namespace OutOfSchool.BusinessLogic.Services.SearchString;
public class SearchStringService : ISearchStringService
{
    private readonly ILogger<SearchStringService> logger;
    private readonly IOptions<SearchStringSettings> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchStringService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="options">
    /// An instance of IOptions that provides access to configuration options.
    /// </param>
    public SearchStringService(
        IOptions<SearchStringSettings> options,
        ILogger<SearchStringService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public string[] SplitSearchString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            logger.LogDebug("Input string is empty or contains only whitespace.");
            return Array.Empty<string>();
        }

        logger.LogDebug("Processing input string: {Input}.", input);
        string[] separators;
        string defaultSeparator = "  ";

        if (options.Value == null ||
            options.Value.Separators == null ||
            options.Value.Separators.Length == 0)
        {
            logger.LogError(
            "Configuration issue with {Settings}: either options are not provided. Using default separators.",
            nameof(SearchStringSettings));
            separators = new string[] { defaultSeparator };
        }
        else
        {
            separators = options.Value.Separators;
        }

        var words = input.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        logger.LogDebug("Split input string into {Length} words.", words.Length);

        return words;
    }
}
