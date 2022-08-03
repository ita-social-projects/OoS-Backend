using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Common.StatusPermissions;

public class ApplicationStatusPermissions : StatusPermissions<ApplicationStatus>
{
    public ApplicationStatusPermissions()
    {
        // deny change status from completed, rejected, left
        DenyStatusChange("all", ApplicationStatus.Completed);
        DenyStatusChange("all", ApplicationStatus.Rejected);
        DenyStatusChange("all", ApplicationStatus.Left);

        // allow change from any to any status
        AllowStatusChange("techadmin");
        AllowStatusChange("provider");

        // allow change from any to left status
        AllowStatusChange("parent", toStatus: ApplicationStatus.Left);
    }

    public void InitDefaultPermissions()
    {
        DenyStatusChange("all", toStatus: ApplicationStatus.AcceptedForSelection);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending);
    }

    public void InitCompetitiveSelectionPermissions()
    {
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.AcceptedForSelection);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.AcceptedForSelection);
    }
}