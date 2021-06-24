using Microsoft.Extensions.Configuration;

namespace OutOfSchool.EmailSender
{
    public class SmtpOptions
    {
        public static string SectionName { get; } = EmailOptions.SectionName + ConfigurationPath.KeyDelimiter + "Smtp";

        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
