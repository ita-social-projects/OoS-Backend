using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.EmailSender.Senders;
using OutOfSchool.EmailSender.Services;
using SendGrid.Extensions.DependencyInjection;

namespace OutOfSchool.EmailSender;

public static class ServiceProviderExtensions
{
    private const string PlaceholderForSendGridApiKey = "x";

    public static IServiceCollection AddEmailSenderService(
        this IServiceCollection services,
        Action<OptionsBuilder<EmailOptions>> emailOptions)
    {
        services.AddTransient<IEmailSenderService, EmailSenderService>();
        
        ArgumentNullException.ThrowIfNull(emailOptions);

        var emailOptionsBuilder = services.AddOptions<EmailOptions>();
        emailOptions(emailOptionsBuilder);
        return services;
    }

    public static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        bool isDevelopment,
        string sendGridApiKey)
    {
        if (isDevelopment && string.IsNullOrWhiteSpace(sendGridApiKey))
        {
            services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
            return services;
        }
        
        services.AddSingleton<IEmailSender, SendGridEmailSender>();
        
        if (string.IsNullOrWhiteSpace(sendGridApiKey))
        {
            sendGridApiKey = PlaceholderForSendGridApiKey;
        }
        services.AddSendGrid(options =>
        {
            options.ApiKey = sendGridApiKey;
            options.HttpErrorAsException = true;
        });

        return services;
    }
}