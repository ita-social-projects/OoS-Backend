using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class RatingDto : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Range(1, 5)]
    public int Rate { get; set; }

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;

    public string FirstName { get; set; }

    public string LastName { get; set; }
}