using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace OutOfSchool.WebApi.Hubs;

public class LocalDistributedHubLifetimeManager<THub> : HubLifetimeManager<THub>, IDisposable
    where THub : Hub
{
    private readonly DefaultHubLifetimeManager<THub> defaultHubLifetimeManager;
    private readonly RedisHubLifetimeManager<THub> redisHubLifetimeManager;
    private readonly ILogger<LocalDistributedHubLifetimeManager<THub>> logger;

    #nullable enable
    [SuppressMessage(
        "ReSharper",
        "ContextualLoggerProblem",
        Justification = "Need exact logger types to instantiate the required types")]
    public LocalDistributedHubLifetimeManager(
        ILogger<LocalDistributedHubLifetimeManager<THub>> logger,
        ILogger<DefaultHubLifetimeManager<THub>> defaultLogger,
        ILogger<RedisHubLifetimeManager<THub>> redisLogger,
        IOptions<RedisOptions> options,
        IHubProtocolResolver hubProtocolResolver,
        IOptions<HubOptions>? globalHubOptions,
        IOptions<HubOptions<THub>>? hubOptions)
    {
        this.logger = logger;
        defaultHubLifetimeManager = new DefaultHubLifetimeManager<THub>(defaultLogger);
        redisHubLifetimeManager = new RedisHubLifetimeManager<THub>(redisLogger, options, hubProtocolResolver, globalHubOptions, hubOptions);
    }

    /// <inheritdoc/>
    public override Task OnConnectedAsync(HubConnectionContext connection)
    {
        try
        {
            Task.WaitAll(
                defaultHubLifetimeManager.OnConnectedAsync(connection),
                redisHubLifetimeManager.OnConnectedAsync(connection));
        }
        catch (AggregateException ex)
        {
            HandleAggregateException(ex);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task OnDisconnectedAsync(HubConnectionContext connection)
    {
        try
        {
            Task.WaitAll(
                defaultHubLifetimeManager.OnDisconnectedAsync(connection),
                redisHubLifetimeManager.OnDisconnectedAsync(connection));
        }
        catch (AggregateException ex)
        {
            HandleAggregateException(ex);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task SendAllAsync(string methodName, object?[] args, CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendAllAsync(methodName, args, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendAllAsync(methodName, args, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendAllExceptAsync(
        string methodName,
        object?[] args,
        IReadOnlyList<string> excludedConnectionIds,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendAllExceptAsync(methodName, args, excludedConnectionIds, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendAllExceptAsync(methodName, args, excludedConnectionIds, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendConnectionAsync(
        string connectionId,
        string methodName,
        object?[] args,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendConnectionAsync(connectionId, methodName, args, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendConnectionAsync(connectionId, methodName, args, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendConnectionsAsync(
        IReadOnlyList<string> connectionIds,
        string methodName,
        object?[] args,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendConnectionsAsync(connectionIds, methodName, args, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendConnectionsAsync(connectionIds, methodName, args, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendGroupAsync(
        string groupName,
        string methodName,
        object?[] args,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendGroupAsync(groupName, methodName, args, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendGroupAsync(groupName, methodName, args, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendGroupsAsync(
        IReadOnlyList<string> groupNames,
        string methodName,
        object?[] args,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendGroupsAsync(groupNames, methodName, args, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendGroupsAsync(groupNames, methodName, args, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendGroupExceptAsync(
        string groupName,
        string methodName,
        object?[] args,
        IReadOnlyList<string> excludedConnectionIds,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendUserAsync(
        string userId,
        string methodName,
        object?[] args,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendUserAsync(userId, methodName, args, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendUserAsync(userId, methodName, args, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task SendUsersAsync(
        IReadOnlyList<string> userIds,
        string methodName,
        object?[] args,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await redisHubLifetimeManager.SendUsersAsync(userIds, methodName, args, cancellationToken);
        }
        catch (RedisConnectionException)
        {
            await defaultHubLifetimeManager.SendUsersAsync(userIds, methodName, args, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override Task AddToGroupAsync(
        string connectionId,
        string groupName,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            Task.WaitAll(
                new[]
                {
                defaultHubLifetimeManager.AddToGroupAsync(connectionId, groupName, cancellationToken),
                redisHubLifetimeManager.AddToGroupAsync(connectionId, groupName, cancellationToken),
                },
                cancellationToken: cancellationToken);
        }
        catch (AggregateException ex)
        {
            HandleAggregateException(ex);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task RemoveFromGroupAsync(
        string connectionId,
        string groupName,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            Task.WaitAll(
                new[]
                {
                    defaultHubLifetimeManager.RemoveFromGroupAsync(connectionId, groupName, cancellationToken),
                    redisHubLifetimeManager.RemoveFromGroupAsync(connectionId, groupName, cancellationToken),
                },
                cancellationToken: cancellationToken);
        }
        catch (AggregateException ex)
        {
            HandleAggregateException(ex);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        redisHubLifetimeManager.Dispose();
    }

    private void HandleAggregateException(AggregateException ex)
    {
        // Only exception that might occur is RedisException
        var redis = ex.InnerExceptions.FirstOrDefault(e => e is RedisException);
        logger.LogError(redis, "Redis not available");
    }
}