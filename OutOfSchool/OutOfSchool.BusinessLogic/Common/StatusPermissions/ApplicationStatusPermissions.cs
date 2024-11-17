using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Common.StatusPermissions;

public class ApplicationStatusPermissions : StatusPermissions<ApplicationStatus>
{
    public void InitDefaultPermissions()
    {
        InitCommonPermissions();

        DenyStatusChange("all", toStatus: ApplicationStatus.AcceptedForSelection);

        // allow statuses for Admin/TechAdmin
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);

        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);

        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);

        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);

        // parent
        AllowStatusChange("parent", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("parent", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Pending);

        // provider
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Rejected);

        // employee
        AllowStatusChange("employee", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Approved);
    }

    public void InitCompetitiveSelectionPermissions()
    {
        InitCommonPermissions();

        // allow statuses for Admin/TechAdmin
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);

        // parent
        AllowStatusChange("parent", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Left);

        // provider
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.AcceptedForSelection);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.AcceptedForSelection);

        // employee
        AllowStatusChange("employee", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("employee", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
    }

    private void InitCommonPermissions()
    {
        // allow statuses for Admin/TechAdmin (MinistryAdmin, RegionAdmin, AreaAdmin the same)
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.Pending);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("techadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);

        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.Pending);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("ministryadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);

        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.Pending);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("regionadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);

        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.Pending);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("areaadmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);

        // allow statuses for Parent
        AllowStatusChange("parent", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Left);
        AllowStatusChange("parent", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);
        AllowStatusChange("parent", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);

        // allow statuses for Provider
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("provider",  fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider",  fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider",  fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider",  fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.StudyingForYears);
    }
}
