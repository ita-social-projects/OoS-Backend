using OutOfSchool.Services.Enums;

namespace OutOfSchool.RazorTemplatesData.Models.Emails;
public class ApplicationStatusViewModel
{
    public const string UaMaleEnding = "ий";
    public const string UaFemaleEnding = "а";
    public const string UaUnspecifiedGenderEnding = "ий/a";

    public ApplicationStatus ApplicationStatus { get; set; }

    public string ChildFullName { get; set; }

    public Gender? ChildGender { get; set; }

    public string UaEnding
    {
        get
        {
            if (ChildGender == Gender.Male)
            {
                return UaMaleEnding;
            }
            else if (ChildGender == Gender.Female)
            {
                return UaFemaleEnding;
            }
            else
            {
                return UaUnspecifiedGenderEnding;
            };
        } 
    }

    public string WorkshopTitle { get; set; }

    public string WorkshopUrl { get; set; }

    public string RejectionMessage { get; set; } = string.Empty;
}
