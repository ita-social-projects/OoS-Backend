namespace OutOfSchool.QuartzJobs.Api.Configuration;

public class QuartzMonitoringOptions
{
    public bool Enabled { get; set; } = true;
    public RedisConfig? Redis { get; set; }

    public class RedisConfig
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? Password { get; set; }
        public int HistoryLength { get; set; }
    }
}
