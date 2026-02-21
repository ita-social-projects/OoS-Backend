using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Redis;

public class RedisForTempSaveConfig
{
    public const string Name = "RedisForTempSave";

    [Required]
    public TimeSpan AbsoluteExpirationRelativeToNowInterval { get; set; }
}