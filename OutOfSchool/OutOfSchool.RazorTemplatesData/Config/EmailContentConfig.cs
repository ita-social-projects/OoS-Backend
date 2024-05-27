namespace OutOfSchool.RazorTemplatesData.Config;

public class EmailContentConfig
{
    private readonly string EmailContentHost;
    private static readonly string EmailContentPath = "/_content/email";

    public string EmailContentUrl => string.Concat(EmailContentHost, EmailContentPath);

    public EmailContentConfig(string emailContentHost)
    {
        EmailContentHost = emailContentHost;
    }
}