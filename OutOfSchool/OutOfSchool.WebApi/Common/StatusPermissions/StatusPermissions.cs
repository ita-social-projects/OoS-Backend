using Grpc.Net.Client.Balancer;

namespace OutOfSchool.WebApi.Common.StatusPermissions;

public class StatusPermissions<T>
    where T : struct
{
    private readonly List<StatusChangePermission<T>> statusPermissionsList = new();

    public void AllowStatusChange(string role, string subRole, T fromStatus = default, T toStatus = default)
    {
        statusPermissionsList.Add(new StatusChangePermission<T>
        {
            Role = role,
            SubRole = subRole,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Allowed = true,
        });
    }

    public void DenyStatusChange(string role, string subRole, T fromStatus = default, T toStatus = default)
    {
        statusPermissionsList.Add(new StatusChangePermission<T>
        {
            Role = role,
            SubRole = subRole,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Allowed = false,
        });
    }

    public bool CanChangeStatus(string role, string subRole, T from, T to)
    {
        var denyResult = statusPermissionsList.Any(p =>
            (p.Role == role || p.Role == "all")
            && (p.SubRole == subRole || p.SubRole == "all")
            && (Convert.ToInt32(p.FromStatus) == Convert.ToInt32(from) || Convert.ToInt32(p.FromStatus) == 0)
            && (Convert.ToInt32(p.ToStatus) == Convert.ToInt32(to) || Convert.ToInt32(p.ToStatus) == 0)
            && !p.Allowed);

        if (denyResult)
        {
            return false;
        }

        var allowResult = statusPermissionsList.Any(p =>
            (p.Role == role || p.Role == "all")
            && (p.SubRole == subRole || p.SubRole == "all")
            && (Convert.ToInt32(p.FromStatus) == Convert.ToInt32(from) || Convert.ToInt32(p.FromStatus) == 0)
            && (Convert.ToInt32(p.ToStatus) == Convert.ToInt32(to) || Convert.ToInt32(p.ToStatus) == 0)
            && p.Allowed);

        return allowResult;
    }
}
