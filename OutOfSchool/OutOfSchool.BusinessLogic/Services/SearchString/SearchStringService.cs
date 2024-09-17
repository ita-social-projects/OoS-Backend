using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config.SearchStringConfig;

namespace OutOfSchool.BusinessLogic.Services.SearchString;
public class SearchStringService : ISearchStringService
{
    private readonly ILogger<SearchStringService> logger;
    private string[] separators;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchStringService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="optionsMonitor">
    /// An instance of IOptionsMonitor that provides access to configuration options and supports change notifications.
    /// It allows the method to retrieve the current value of configuration settings and react to updates in real time.
    /// </param>
    public SearchStringService(
        IOptionsMonitor<SearchStringSettings> optionsMonitor,
        ILogger<SearchStringService> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (optionsMonitor == null || optionsMonitor.CurrentValue == null)
        {
            logger.LogError(
                "{SettingsType} or its CurrentValue is null. Using default separators.",
                nameof(IOptionsMonitor<SearchStringSettings>));

            separators = [" "];
        }

        // Initialize the separators with the current configuration.
        separators = optionsMonitor.CurrentValue.Separators;

        // Subscribe to the configuration change event and update separators when the settings change.
        optionsMonitor.OnChange(settings =>
        {
            logger.LogDebug("Configuration SearchStringSettings changed: Separators updated.");
            separators = optionsMonitor.CurrentValue.Separators;
        });
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

        var words = input.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        logger.LogDebug("Split input into {Length} words.", words.Length);

        return words;
    }
}
