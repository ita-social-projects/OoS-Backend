using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Redis
{
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

        [Range(1, 12, ErrorMessage = "AbsoluteExpirationRelativeToNowFromHours must be between then 1 and 12.")]
        public int AbsoluteExpirationRelativeToNowFromHours { get; set; }

        [Range(1, 60, ErrorMessage = "SlidingExpirationFromMinutes must be between then 1 and 60.")]
        public int SlidingExpirationFromMinutes { get; set; }

        [Range(1, 1800, ErrorMessage = "SecondsChekingIsWorking must be between then 1 and 1800.")]
        public int SecondsChekingIsWorking { get; set; }
    }
}
