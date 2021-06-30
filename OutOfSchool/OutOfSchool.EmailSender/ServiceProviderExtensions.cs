using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OutOfSchool.EmailSender
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddEmailSender(
            this IServiceCollection services,
            Action<OptionsBuilder<EmailOptions>> emailOptions,
            Action<OptionsBuilder<SmtpOptions>> smtpOptions)
        {
            services.AddSingleton<IEmailSender, EmailSender>();
            if (emailOptions == null)
            {
                throw new ArgumentNullException(nameof(emailOptions));
            }

            if (smtpOptions == null)
            {
                throw new ArgumentNullException(nameof(smtpOptions));
            }

            var emailOptionsBuilder = services.AddOptions<EmailOptions>();
            var smtpOptionsBuilder = services.AddOptions<SmtpOptions>();
            emailOptions(emailOptionsBuilder);
            smtpOptions(smtpOptionsBuilder);
            return services;
        }
    }
}
