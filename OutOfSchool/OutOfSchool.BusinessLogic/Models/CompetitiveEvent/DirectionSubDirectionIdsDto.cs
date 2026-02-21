namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

/// <summary>
/// Represents a pairing of direction and sub-direction identifiers.
/// </summary>
public class DirectionSubDirectionIdsDto
{
    /// <summary>
    /// Gets or sets the direction identifier.
    /// </summary>
    public long DirectionId { get; set; }

    /// <summary>
    /// Gets or sets the sub-direction identifier.
    /// </summary>
    public long SubDirectionId { get; set; }
}
