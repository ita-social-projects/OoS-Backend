using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.RazorTemplatesData.Config;

public static class ServiceProviderExtensions
{
    public static IServiceCollection AddEmailRendererConfiguration(
        this IServiceCollection services,
        EmailContentConfig emailContentConfig)
    {
        services.AddSingleton(emailContentConfig);
        return services;
    }
}