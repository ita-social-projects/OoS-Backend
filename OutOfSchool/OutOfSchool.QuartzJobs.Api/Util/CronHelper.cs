using Quartz;

namespace OutOfSchool.QuartzJobs.Api.Util;
public static class CronHelper
{
    public static List<DateTimeOffset> GetUpcomingExecutions(string cron, int count)
    {
        var result = new List<DateTimeOffset>();
        var now = DateTimeOffset.UtcNow;

        if (!CronExpression.IsValidExpression(cron))
            return result;

        var expression = new CronExpression(cron);

        for (int i = 0; i < count; i++)
        {
            var next = expression.GetNextValidTimeAfter(now);
            if (next is null) break;

            result.Add(next.Value.ToLocalTime());
            now = next.Value;
        }

        return result;
    }
}
