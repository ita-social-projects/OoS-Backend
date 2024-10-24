using OutOfSchool.Services.Enums;
using OutOfSchool.BusinessLogic.Enums;

namespace OutOfSchool.BusinessLogic.Models.Application;

public class ApplicationFilter : SearchStringFilter
{
    public IEnumerable<ApplicationStatus> Statuses { get; set; } = null;

    public bool OrderByDateAscending { get; set; } = true;

    public bool OrderByAlphabetically { get; set; } = true;

    public bool OrderByStatus { get; set; } = true;

    public ShowApplications Show { get; set; } = ShowApplications.All;

    public IEnumerable<Guid> Workshops { get; set; } = null;

    public IEnumerable<Guid> Children { get; set; } = null;
}