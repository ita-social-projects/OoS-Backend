﻿using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Config;

public class ElasticConfig
{
    public const string Name = "Elasticsearch";

    public bool EnsureIndex { get; set; }

    public bool EnableDebugMode { get; set; }

    [Required]
    public string WorkshopIndexName { get; set; }

    public string User { get; set; }

    public string Password { get; set; }

    public List<string> Urls { get; set; }

    public int PingIntervalSeconds { get; set; }
}