using Microsoft.Extensions.Hosting;

namespace OutOfSchool.AuthCommon.Services;
/// <summary>
/// Hosted service that initializes system user.
/// </summary>
public class SystemUserInitializerHostedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<SystemUserInitializerHostedService> logger;

    public SystemUserInitializerHostedService(IServiceProvider serviceProvider, ILogger<SystemUserInitializerHostedService> logger)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ISystemUserInitializer>();

        try
        {
            await initializer.EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogDebug(ex, "System user initialization was cancelled.");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"System user initialization failed.", ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
