using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Common.StatusPermissions;

public class ApplicationStatusPermissions : StatusPermissions<ApplicationStatus>
{
    public ApplicationStatusPermissions()
    {
        // deny change status from completed, rejected, left
        DenyStatusChange("all", "all", ApplicationStatus.Completed);
        DenyStatusChange("all", "all", ApplicationStatus.Rejected);
        DenyStatusChange("all", "all", ApplicationStatus.Left);

        // allow change from any to any status
        AllowStatusChange("admin", "all");
        AllowStatusChange("techadmin", "all");
        AllowStatusChange("provider", "DeputyAdmin");

        // allow change from any to left status
        AllowStatusChange("parent", "all", toStatus: ApplicationStatus.Left);
    }

    public void InitDefaultPermissions()
    {
        DenyStatusChange("all", "all", toStatus: ApplicationStatus.AcceptedForSelection);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Pending);
    }

    public void InitCompetitiveSelectionPermissions()
    {
        AllowStatusChange("provider", "ProviderAdmin", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.AcceptedForSelection);
        AllowStatusChange("provider", "ProviderAdmin", fromStatus: ApplicationStatus.AcceptedForSelection);
    }
}