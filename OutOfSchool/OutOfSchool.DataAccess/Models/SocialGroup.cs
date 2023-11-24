using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class SocialGroup : IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string NameEn { get; set; } = string.Empty;

    public virtual List<Child> Children { get; set; }
}