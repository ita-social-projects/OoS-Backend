using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Config;

public class GRPCConfig
{
    public const string Name = "GRPC";

    public bool Enabled { get; set; }

    [Required]
    public string Server { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Port must be greater then 0.")]
    public int Port { get; set; }

    public int ChannelMaxAttempts { get; set; }

    [Required]
    public TimeSpan ChannelInitialBackoffInterval { get; set; }

    [Required]
    public TimeSpan ChannelMaxBackoffInterval { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "ChannelBackoffMultiplier must be greater then 1.")]
    public double ChannelBackoffMultiplier { get; set; }
}