using OutOfSchool.Licenses.Models;

namespace OutOfSchool.Licenses.Services;
public interface IPackagesConverter
{
    string Convert(Dictionary<string, IEnumerable<PackageWithLicense>> data);
}