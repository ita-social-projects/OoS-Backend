using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid.Extensions.DependencyInjection;

namespace OutOfSchool.EmailSender;

public static class ServiceProviderExtensions
{
    public const string PlaceholderForSendGridApiKey = "x";

    public static IServiceCollection AddEmailSenderService(
        this IServiceCollection services,
        bool isDevelopment,
        string sendGridApiKey,
        Action<OptionsBuilder<EmailOptions>> emailOptions)
    {
        if (isDevelopment && string.IsNullOrWhiteSpace(sendGridApiKey))
        {
            services.AddTransient<IEmailSenderService, DevEmailSender>();
            return services;
        }

        if (string.IsNullOrWhiteSpace(sendGridApiKey))
        {
            sendGridApiKey = PlaceholderForSendGridApiKey;
        }

        services.AddSendGrid(options =>
        {
            options.ApiKey = sendGridApiKey;
            options.HttpErrorAsException = true;
        });

        services.AddTransient<IEmailSenderService, EmailSenderService>();
        if (emailOptions == null)
        {
            throw new ArgumentNullException(nameof(emailOptions));
        }

        var emailOptionsBuilder = services.AddOptions<EmailOptions>();
        emailOptions(emailOptionsBuilder);
        return services;
    }
}