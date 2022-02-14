using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Config
{
    public class ElasticConfig
    {
        public const string Name = "Elasticsearch";

        public bool EnsureIndex { get; set; }

        public bool EnableDebugMode { get; set; }

        [Required]
        public string DefaultIndex { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public List<string> Urls { get; set; }

        public int PingIntervalSeconds { get; set; }
    }
}