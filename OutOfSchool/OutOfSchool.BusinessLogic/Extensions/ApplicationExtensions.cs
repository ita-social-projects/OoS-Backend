using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Extensions;

// TODO: Methods of this class are used only in the ApplicationExtensionTests class
public static class ApplicationExtensions
{
    public static int AmountOfPendingApplications(this IEnumerable<Application> entities)
    {
        return entities.Count(x =>
            x.Status == ApplicationStatus.Pending
            && !x.IsDeleted
            && x.Child != null
            && !x.Child.IsDeleted
            && x.Parent != null
            && !x.Parent.IsDeleted);
    }

    public static int TakenSeats(this IEnumerable<Application> entities)
    {
        return entities.Count(x =>
            (x.Status == ApplicationStatus.Approved || x.Status == ApplicationStatus.StudyingForYears)
            && !x.IsDeleted
            && x.Child != null
            && !x.Child.IsDeleted
            && x.Parent != null
            && !x.Parent.IsDeleted);
    }
}
