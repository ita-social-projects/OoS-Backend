using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class Rating : IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }

    [Range(1, 5)]
    public int Rate { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "The EntityId field should be bigger than 0")]
    public Guid EntityId { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "The ParentId field should be bigger than 0")]
    public Guid ParentId { get; set; }

    [Required]
    public DateTimeOffset CreationTime { get; set; }

    public virtual Parent Parent { get; set; }
}