using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Extensions;

public static class ApplicationExtensions
{
    public static int AmountOfPendingApplications(this IEnumerable<Application> entities)
    {
        return entities.Count(x =>
            x.Status == ApplicationStatus.Pending
            && !x.IsDeleted
            && (x.Child == null || !x.Child.IsDeleted)
            && (x.Parent == null || !x.Parent.IsDeleted));
    }

    public static int TakenSeats(this IEnumerable<Application> entities)
    {
        return entities.Count(x =>
            (x.Status == ApplicationStatus.Approved || x.Status == ApplicationStatus.StudyingForYears)
            && !x.IsDeleted
            && (x.Child == null || !x.Child.IsDeleted)
            && (x.Parent == null || !x.Parent.IsDeleted));
    }
}
