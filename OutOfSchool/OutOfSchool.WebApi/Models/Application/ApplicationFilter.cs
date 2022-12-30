using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Application;

public class ApplicationFilter : OffsetFilter
{
    public IEnumerable<ApplicationStatus> Statuses { get; set; } = null;

    public bool OrderByDateAscending { get; set; } = true;

    public bool OrderByAlphabetically { get; set; } = true;

    public bool OrderByStatus { get; set; } = true;

    public bool ShowBlocked { get; set; } = false;

    public IEnumerable<Guid> Workshops { get; set; } = null;

    public IEnumerable<Guid> Children { get; set; } = null;
}