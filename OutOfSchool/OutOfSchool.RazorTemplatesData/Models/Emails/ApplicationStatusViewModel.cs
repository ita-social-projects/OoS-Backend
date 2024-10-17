namespace OutOfSchool.RazorTemplatesData.Models.Emails;
public class ApplicationStatusViewModel
{
    public string ChildFullName { get; set; }

    public string UaEnding { get; set; }

    public string WorkshopTitle { get; set; }

    public string WorkshopUrl { get; set; }

    public string RejectionMessage { get; set; } = string.Empty;
}
