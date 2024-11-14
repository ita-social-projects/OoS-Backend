#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace OutOfSchool.Encryption.Services;

public class IoOperationsService : IIoOperationsService
{
    public bool Exists([NotNullWhen(true)] string? path) => File.Exists(path);

    public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);

    public Stream GetMemoryStreamFromBytes(byte[] buffer) => new MemoryStream(buffer);

    public Stream GetFileStreamFromPath([NotNull] string path, FileMode mode) => new FileStream(path, mode);
}