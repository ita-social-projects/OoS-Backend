namespace OutOfSchool.WebApi.Config.Images;

/// <summary>
/// Describes image options for some Entity.
/// </summary>
/// <typeparam name="TEntity">This is an entity for which you can operate with images.</typeparam>
public class ImageOptions<TEntity>
{
    /// <summary>
    /// Gets or sets the minimum width in pixels.
    /// </summary>
    public int MinWidthPixels { get; set; }

    /// <summary>
    /// Gets or sets the maximum width in pixels.
    /// </summary>
    public int MaxWidthPixels { get; set; }

    /// <summary>
    /// Gets or sets the minimum height in pixels.
    /// </summary>
    public int MinHeightPixels { get; set; }

    /// <summary>
    /// Gets or sets the maximum height in pixels.
    /// </summary>
    public int MaxHeightPixels { get; set; }

    /// <summary>
    /// Gets or sets the min ratio between width and height.
    /// </summary>
    public float MinWidthHeightRatio { get; set; }

    /// <summary>
    /// Gets or sets the max ratio between width and height.
    /// </summary>
    public float MaxWidthHeightRatio { get; set; }

    /// <summary>
    /// Gets or sets the maximum image size in bytes.
    /// </summary>
    public int MaxSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the supported image formats.
    /// </summary>
    public List<string> SupportedFormats { get; set; }
}