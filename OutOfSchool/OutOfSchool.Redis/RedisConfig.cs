using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Redis;

public class RedisConfig
{
    public const string Name = "Redis";

    public bool Enabled { get; set; }

    [Required]
    public string Server { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Port must be greater then 0.")]
    public int Port { get; set; }

    [Required]
    public TimeSpan AbsoluteExpirationRelativeToNowInterval { get; set; }

    [Required]
    public TimeSpan SlidingExpirationInterval { get; set; }

    [Required]
    public TimeSpan CheckAlivePollingInterval { get; set; }

    public string GetRedisConnectionString()
    {
        return $"{Server}:{Port},password={Password}";
    }
}