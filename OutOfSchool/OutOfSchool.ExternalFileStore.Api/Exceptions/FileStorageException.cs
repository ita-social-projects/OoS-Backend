using System.Runtime.Serialization;

namespace OutOfSchool.ExternalFileStore.Exceptions;

/// <summary>
/// The ImageStorageException is thrown when something has happened while trying
/// to work with image storage.
/// </summary>
[Serializable]
public class FileStorageException : Exception
{
    public FileStorageException()
    {
    }

    public FileStorageException(Exception ex)
        : this("Unhandled exception", ex)
    {
    }

    public FileStorageException(string message)
        : base(message)
    {
    }

    public FileStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected FileStorageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}