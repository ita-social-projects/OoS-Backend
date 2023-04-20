namespace OutOfSchool.RazorTemplatesData.Models.Emails;

public class AdminInvitationViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmationUrl { get; set; }
}