using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config.SearchString;

namespace OutOfSchool.BusinessLogic.Services.SearchString;
public class SearchStringService : ISearchStringService
{
    private const string DefaultSeparator = " ";
    private readonly ILogger<SearchStringService> logger;
    private readonly IOptions<SearchStringOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchStringService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="options">
    /// An instance of IOptions that provides access to configuration options.
    /// </param>
    public SearchStringService(
        IOptions<SearchStringOptions> options,
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

        // Use the separators from options if available, otherwise default to space.
        string[] separators = options.Value?.Separators;
        if (separators == null || separators.Length == 0)
        {
            logger.LogError(
                "Configuration issue with {Settings}: options or separators are not provided. Using default separators.",
                nameof(SearchStringOptions));
            separators = [DefaultSeparator];
        }

        return input.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
