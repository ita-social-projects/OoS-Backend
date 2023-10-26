using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Redis;

public class MemoryCacheConfig
{
    public const string Name = "MemoryCache";

    [Required]
    public TimeSpan AbsoluteExpirationRelativeToNowInterval { get; set; }

    [Required]
    public TimeSpan SlidingExpirationInterval { get; set; }

    [Required]
    public int Size { get; set; }
}
