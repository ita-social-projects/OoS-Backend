using System.Collections.Generic;

namespace OutOfSchool.Services.Models;
public class Tag : IKeyedEntity<long>
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public virtual List<Workshop> Workshops { get; set; }
}
