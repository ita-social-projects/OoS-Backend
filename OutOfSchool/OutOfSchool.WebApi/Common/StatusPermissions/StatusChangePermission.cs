using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Common.StatusPermissions;

public class StatusChangePermission<T>
{
    /// <summary>
    /// Contains the 'role' name.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// From status change
    /// </summary>
    public T FromStatus { get; set; }

    /// <summary>
    /// To status change
    /// </summary>
    public T ToStatus { get; set; }

    /// <summary>
    /// allow change status
    /// </summary>
    public bool Allowed { get; set; }
}