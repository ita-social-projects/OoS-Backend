using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Common.StatusPermissions;

public class ApplicationStatusPermissions : StatusPermissions<ApplicationStatus>
{
    public ApplicationStatusPermissions()
    {
        // allow statuses for Admin/TechAdmin
        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.Pending);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.Pending);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);

        // allow statuses for Parent
        AllowStatusChange("parent", "all", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Left);
        AllowStatusChange("parent", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);
        AllowStatusChange("parent", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);

        // allow statuses for Provider
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.StudyingForYears);
    }

    public void InitDefaultPermissions()
    {
        DenyStatusChange("all", "all", toStatus: ApplicationStatus.AcceptedForSelection);

        // allow statuses for Admin/TechAdmin
        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Completed);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Left);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Left);

        // parent
        AllowStatusChange("parent", "all", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("parent", "all", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Pending);

        // provider
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Left, toStatus: ApplicationStatus.Rejected);

        AllowStatusChange("provider", "providerdeputy", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", "providerdeputy", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Approved);

        AllowStatusChange("provider", "providerddmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Approved);
    }

    public void InitCompetitiveSelectionPermissions()
    {
        // allow statuses for Admin/TechAdmin
        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);

        AllowStatusChange("admin", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("techadmin", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);

        // parent
        AllowStatusChange("parent", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Left);

        // provider
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.AcceptedForSelection);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Approved);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Completed);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("provider", "all", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.StudyingForYears);

        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Pending, toStatus: ApplicationStatus.Rejected);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("provider", "none", fromStatus: ApplicationStatus.Rejected, toStatus: ApplicationStatus.AcceptedForSelection);

        AllowStatusChange("provider", "providerdeputy", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("provider", "providerdeputy", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("provider", "providerdeputy", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Pending);

        AllowStatusChange("provider", "providerddmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("provider", "providerddmin", fromStatus: ApplicationStatus.Approved, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("provider", "providerddmin", fromStatus: ApplicationStatus.StudyingForYears, toStatus: ApplicationStatus.Pending);
        AllowStatusChange("provider", "providerddmin", fromStatus: ApplicationStatus.AcceptedForSelection, toStatus: ApplicationStatus.StudyingForYears);
    }
}
