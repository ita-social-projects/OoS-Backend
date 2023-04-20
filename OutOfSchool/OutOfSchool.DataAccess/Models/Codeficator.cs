using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class CATOTTG : IKeyedEntity<long>
{
    public long Id { get; set; }

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    public long? ParentId { get; set; }

    [MaxLength(3)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; } = default;

    public double Longitude { get; set; } = default;

    public ulong GeoHash { get; set; } = default;

    public bool NeedCheck { get; set; } = default;

    public int Order { get; set; } = default;

    public virtual CATOTTG Parent { get; set; }
}