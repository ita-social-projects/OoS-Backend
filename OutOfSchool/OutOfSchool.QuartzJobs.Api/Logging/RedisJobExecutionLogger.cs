using Microsoft.Extensions.Options;
using OutOfSchool.QuartzJobs.Api.Configuration;
using OutOfSchool.QuartzJobs.Api.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace OutOfSchool.QuartzJobs.Api.Logging;

public class RedisJobExecutionLogger : IJobExecutionLogger
{
    private readonly IDatabase redis;
    private readonly int historyLength;

    public RedisJobExecutionLogger(IConnectionMultiplexer connection, IOptions<QuartzMonitoringOptions> options)
    {
        redis = connection.GetDatabase();
        historyLength = options.Value.Redis?.HistoryLength ?? 10;
    }

    public async Task LogAsync(JobExecutionInfo info)
    {
        var key = GetKey(info.JobName);
        var json = JsonSerializer.Serialize(info);

        await redis.ListLeftPushAsync(key, json);
        await redis.ListTrimAsync(key, 0, historyLength - 1);
    }

    public async Task<IReadOnlyList<JobExecutionInfo>> GetHistoryAsync(string jobName)
    {
        var key = GetKey(jobName);
        var items = await redis.ListRangeAsync(key);

        return items
            .Select(x => JsonSerializer.Deserialize<JobExecutionInfo>(x!)!)
            .Where(x => x != null)
            .ToList();
    }

    public async Task<IReadOnlyList<JobExecutionInfo>> GetFailedAsync(string jobName)
    {
        var all = await GetHistoryAsync(jobName);
        return all.Where(x => !x.WasSuccessful).ToList();
    }

    private static string GetKey(string jobName) =>
        $"monitoring:jobs:{jobName}:history";
}
