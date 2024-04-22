namespace OutOfSchool.Licenses.Util;

public static class Validators
{
    public static bool IsFileNameValid(string? fileName) =>
        IsDirectoryNameValid(fileName) && !string.IsNullOrWhiteSpace(Path.GetFileName(fileName));

    public static bool IsDirectoryNameValid(string? directoryName) =>
        Path.GetDirectoryName(directoryName) is not null
        || DriveInfo.GetDrives().Any(d => d.Name.Equals(directoryName, StringComparison.InvariantCultureIgnoreCase));
}
