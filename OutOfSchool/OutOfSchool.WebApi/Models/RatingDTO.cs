using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

public class RatingDto
{
    public long Id { get; set; }

    [Range(1, 5)]
    public int Rate { get; set; }

    [Required]
    [Range(1, 2, ErrorMessage = "The type field should be 1 or 2")]
    public RatingType Type { get; set; }

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;

    public string FirstName { get; set; }

    public string LastName { get; set; }
}