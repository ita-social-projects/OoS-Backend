namespace OutOfSchool.EmailService
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }
        string PopServer { get; }
        int PopPort { get; }
        string PopUsername { get; set; }
        string PopPassword { get; set; }
    }
}
