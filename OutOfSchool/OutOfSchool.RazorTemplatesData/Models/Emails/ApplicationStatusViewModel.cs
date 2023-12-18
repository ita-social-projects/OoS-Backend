
using OutOfSchool.Services.Enums;

namespace OutOfSchool.RazorTemplatesData.Models.Emails;
public class ApplicationStatusViewModel
{
    public ApplicationStatus ApplicationStatus { get; set; }

    public string ChildFullName { get; set; }

    public Gender? ChildGender { get; set; }

    public string WorkshopTitle { get; set; }

    public string WorkshopUrl { get; set; }

    public string RejectionMessage { get; set; } = string.Empty;

}
