namespace OutOfSchool.ExternalFileStore.Models;

public class StorageObject
{
    public string Name { get; set; }
    public string ContentType { get; set; }
    public ulong Size { get; set; }
    
    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? LastModified { get; set; }
} 
