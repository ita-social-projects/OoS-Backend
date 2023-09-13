using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class AverageRating : IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }

    public float Rate { get; set; }

    public int RateQuantity { get; set; }

    [Required]
    public Guid EntityId { get; set; }
}
