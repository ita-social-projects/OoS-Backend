using OutOfSchool.Licenses.Models;

namespace OutOfSchool.Licenses.Services;

public class TextFileWriter : IFileWriter
{
    private readonly string fileName;
    private readonly IPackagesConverter packagesConverter;

    public TextFileWriter(string fileName, IPackagesConverter packagesConverter)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException($"Invalid file name: '{nameof(fileName)}'");
        }

        ArgumentNullException.ThrowIfNull(packagesConverter);
        
        this.fileName = fileName;
        this.packagesConverter = packagesConverter;
    }

    public void Write(Dictionary<string, IEnumerable<PackageWithLicense>> data) =>
        File.WriteAllText(fileName, packagesConverter.Convert(data));
}
