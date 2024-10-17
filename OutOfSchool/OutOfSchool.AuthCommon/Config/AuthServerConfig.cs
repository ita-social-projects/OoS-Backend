namespace OutOfSchool.AuthCommon.Config;

public class AuthServerConfig
{
    public const string Name = "Identity";

    public Uri Authority { get; set; }

    public string RedirectToStartPageUrl { get; set; }

    public string RedirectFromEmailConfirmationUrl { get; set; }
}