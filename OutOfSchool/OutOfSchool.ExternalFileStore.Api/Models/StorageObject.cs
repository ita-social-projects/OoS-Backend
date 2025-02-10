namespace OutOfSchool.ExternalFileStore.Models;

/// <summary>
/// Represents metadata for an object stored in external storage.
/// </summary>
public class StorageObject
{
    /// <summary>
    /// Gets or sets the name of the storage object.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the content type (MIME type) of the storage object.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the size of the storage object in bytes.
    /// </summary>
    public ulong Size { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the storage object.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the modification date of the storage object.
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }
}