using Microsoft.Extensions.Hosting;

namespace OutOfSchool.AuthCommon.Services;
/// <summary>
/// Hosted service that initializes system user.
/// </summary>
public class SystemUserInitializerHostedService<TUser> : IHostedService
    where TUser : User, new()
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<SystemUserInitializerHostedService<TUser>> logger;

    public SystemUserInitializerHostedService(IServiceProvider serviceProvider, ILogger<SystemUserInitializerHostedService<TUser>> logger)
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
        catch (OperationCanceledException)
        {
            logger.LogInformation("System user initialization was cancelled.");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the system user.");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}
