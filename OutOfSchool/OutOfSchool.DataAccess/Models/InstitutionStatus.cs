using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class InstitutionStatus : IKeyedEntity<long>, ISoftDeleted
{
    public InstitutionStatus()
    {
        Providers = new List<Provider>();
    }

    public long Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public virtual IReadOnlyCollection<Provider> Providers { get; set; }
}