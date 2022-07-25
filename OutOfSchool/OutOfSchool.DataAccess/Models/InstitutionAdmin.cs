using System;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class InstitutionAdmin : IKeyedEntity<long>
{
    public long Id { get; set; }

    public string UserId { get; set; }

    public Guid InstitutionId { get; set; }

    public virtual Institution Institution { get; set; }

    public virtual User User { get; set; }
}