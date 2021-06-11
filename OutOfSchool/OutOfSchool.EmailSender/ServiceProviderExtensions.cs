using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.EmailSender
{
    public static class ServiceProviderExtensions
    {
        public static void AddEmailSender(this IServiceCollection services)
        {
            SmtpConfiguration smtpConfiguration = new SmtpConfiguration()
            {
                Server = "smtp.gmail.com",
                Port = 465,
                Username = "OoS.Backend.Test.Server@gmail.com",
                Password = "00$.@Server2021",
            };
            services.AddSingleton(smtpConfiguration);
            services.AddSingleton<IEmailSender, EmailSender>();
        }
    }
}
