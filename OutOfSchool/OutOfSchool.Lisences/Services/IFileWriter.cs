using OutOfSchool.Licenses.Models;

namespace OutOfSchool.Licenses.Services;

public interface IFileWriter
{
    void Write(Dictionary<string, IEnumerable<PackageWithLicense>> data);
}