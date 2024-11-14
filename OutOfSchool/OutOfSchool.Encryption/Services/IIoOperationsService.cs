#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace OutOfSchool.Encryption.Services;

public interface IIoOperationsService
{
    public bool Exists([NotNullWhen(true)] string? path);

    public byte[] ReadAllBytes(string path);

    public Stream GetMemoryStreamFromBytes(byte[] buffer);

    public Stream GetFileStreamFromPath([NotNull] string path, FileMode mode);
}