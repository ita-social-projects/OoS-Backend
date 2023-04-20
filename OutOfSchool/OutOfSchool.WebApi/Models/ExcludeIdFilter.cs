namespace OutOfSchool.WebApi.Models;

/// <inheritdoc/>>
public class ExcludeIdFilter : OffsetFilter
{
    /// <summary>
    /// Gets or sets filter to exclude entity by excluded id.
    /// </summary>
    public Guid? ExcludedId { get; set; }
}
