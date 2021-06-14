using System;
using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.EmailSender
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddEmailSender(
            this IServiceCollection services,
            Action<SmtpOptions> options)
        {
            services.AddSingleton<IEmailSender, EmailSender>();
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Please provide options for EmailSender");
            }
            services.Configure(options);
            return services;
        }
    }
}
