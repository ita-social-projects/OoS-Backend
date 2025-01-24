using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Redis;

public class RedisForDraftConfig
{
    public const string Name = "RedisForDraft";

    [Required]
    public TimeSpan AbsoluteExpirationRelativeToNowInterval { get; set; }
}