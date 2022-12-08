using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class ProviderType : IKeyedEntity<long>
{
    public long Id { get; set; }
    
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    public virtual List<Provider> Providers { get; set; }
}