using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.Redis
{
    public class RedisConfig
    {
        public const string Name = "Redis";

        public bool Enabled { get; set; }

        public string Server { get; set; }

        public int Port { get; set; }

        public int AbsoluteExpirationRelativeToNowFromHours { get; set; }

        public int SlidingExpirationFromMinutes { get; set; }

        public int SecondsChekingIsWorking { get; set; }
    }
}
