using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;
public class Tag : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(60)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(60)]
    [MinLength(1)]
    public string NameEn { get; set; } = string.Empty;

    public virtual List<Workshop> Workshops { get; set; }
}
