using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.EmailSender
{
    public static class ServiceProviderExtensions
    {
        public static void AddEmailSender(this IServiceCollection services)
        {
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Server = "",
                Port = 0,
                Username = "",
                Password = "",
            };
            services.AddSingleton(smtpConfiguration);
            services.AddSingleton<IEmailSender, EmailSender>();
        }
    }
}
