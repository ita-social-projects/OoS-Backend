namespace OutOfSchool.SportsRegistryApiClient.Models.External;

/// <summary>
/// Represents filter criteria for retrieving sports sections from the Sports Registry.
/// </summary>
public class ExternalSportsSectionFilter
{
    /// <summary>
    /// The lower bound of the update date filter (inclusive).
    /// If null, filtering from this side is not applied.
    /// </summary>
    public DateTimeOffset? UpdatedAtFrom { get; set; }

    /// <summary>
    /// The upper bound of the update date filter (inclusive).
    /// If null, filtering from this side is not applied.
    /// </summary>
    public DateTimeOffset? UpdatedAtTo { get; set; }
}
