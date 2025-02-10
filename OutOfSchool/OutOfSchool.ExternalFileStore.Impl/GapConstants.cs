namespace OutOfSchool.ExternalFileStore;

/// <summary>
/// Contains constants of all date time gaps which are used to exclude objects have created recently
/// to be sure that all processes including objects which are being synchronized were finished.
/// </summary>
public static class GapConstants
{
    public const int GcpImagesSynchronizationDateTimeAddMinutesGap = -60;
}